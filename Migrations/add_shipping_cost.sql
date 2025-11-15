   -- Add shipping_cost column to sales table
ALTER TABLE sales ADD COLUMN IF NOT EXISTS shipping_cost NUMERIC(18,2) NOT NULL DEFAULT 0;
