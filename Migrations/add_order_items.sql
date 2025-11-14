-- Migration: Add OrderItems support
-- This migration adds support for multiple items per order

-- Create order_items table
CREATE TABLE IF NOT EXISTS order_items (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    product_id UUID NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    total_price DECIMAL(18,2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_order_items_order
        FOREIGN KEY (order_id)
        REFERENCES orders(id)
        ON DELETE CASCADE,
        
    CONSTRAINT fk_order_items_product
        FOREIGN KEY (product_id)
        REFERENCES products(id)
        ON DELETE RESTRICT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_order_items_order_id ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_order_items_product_id ON order_items(product_id);

-- Add new columns to orders table
ALTER TABLE orders
    ADD COLUMN IF NOT EXISTS subtotal DECIMAL(18,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_percentage DECIMAL(18,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_amount DECIMAL(18,2) DEFAULT 0;

-- Migrate existing orders to the new structure
-- For each existing order, create one order_item
INSERT INTO order_items (id, order_id, product_id, product_name, quantity, unit_price, total_price, created_at)
SELECT 
    gen_random_uuid(),
    id,
    product_id,
    product_name,
    quantity,
    unit_price,
    total_amount,
    created_at
FROM orders
WHERE product_id IS NOT NULL
ON CONFLICT DO NOTHING;

-- Update orders subtotal to match total_amount for existing orders
UPDATE orders
SET subtotal = total_amount,
    discount_percentage = 0,
    discount_amount = 0
WHERE subtotal IS NULL OR subtotal = 0;

-- Now we can drop the old columns (optional - keep for backwards compatibility initially)
-- ALTER TABLE orders DROP COLUMN IF EXISTS product_id;
-- ALTER TABLE orders DROP COLUMN IF EXISTS product_name;
-- ALTER TABLE orders DROP COLUMN IF EXISTS quantity;
-- ALTER TABLE orders DROP COLUMN IF EXISTS unit_price;
