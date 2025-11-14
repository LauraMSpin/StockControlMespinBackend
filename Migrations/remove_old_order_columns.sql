-- Migration: Remove old single-product columns from orders table
-- Run this AFTER confirming that order_items table is working correctly

-- Drop old single-product columns that are no longer used
ALTER TABLE orders DROP COLUMN IF EXISTS product_id;
ALTER TABLE orders DROP COLUMN IF EXISTS product_name;
ALTER TABLE orders DROP COLUMN IF EXISTS quantity;
ALTER TABLE orders DROP COLUMN IF EXISTS unit_price;

-- Verify the changes
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'orders' 
ORDER BY ordinal_position;
