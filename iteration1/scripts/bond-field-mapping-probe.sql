-- Read-only probe for the STC bond rating / valuation field mapping.
--
-- Verified against the QA shared database (PostgreSQL 10.23 / TBase).
-- Run with a read-only session; see iteration1/docs/stc-bond-field-mapping.zh-CN.md
-- for the resulting mapping and the actual observed values.
--
-- Connection parameters must come from the local gitignored configuration or an
-- environment variable.  Do NOT commit host, account or password here.
--
--   SET SESSION CHARACTERISTICS AS TRANSACTION READ ONLY;
--
-- Every statement below is a SELECT: this script cannot modify the shared instance.

-- 1. Where do rating / valuation live?
SELECT table_name, column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND (column_name ILIKE '%rating%' OR column_name ILIKE '%valuat%')
ORDER BY table_name, column_name;

-- 2. Inventory of the two tables that matter.
SELECT count(*) AS bond_rows FROM public.bond;

SELECT count(*) AS valuation_rows, count(DISTINCT bond_id) AS distinct_bonds
FROM public.china_bond_valuation;

-- 3. Confirm the code-format difference (bare vs exchange-suffixed).
SELECT bond_id, bond_key, mkt_type, bond_subtype
FROM public.bond
WHERE bond_id IN ('250001', '269152', '282353', '282644')
ORDER BY bond_id;

-- 4. Rating for the target bond (250001).
SELECT bond_id, bond_key, mkt_type, bond_subtype, bond_type,
       rating_current, issuer_rating_current,
       cdc_implied_rating, csi_implied_rating,
       rating_date, maturity_date, evaluation_date
FROM public.bond
WHERE bond_id = '250001';

-- 5. Valuation for the target bond.  Expect 0 rows: 250001 is a PPN with no
--    ChinaBond coverage (evaluation_date IS NULL).  Both join styles are tried.
SELECT bond_id, listed_market, valuation_date, valuation_net_price, valuation_yield,
       val_modified_duration, val_convexity, val_basis_point_value, status
FROM public.china_bond_valuation
WHERE bond_id = '250001.IB';

SELECT count(*) AS valuation_rows_via_bond_key
FROM public.china_bond_valuation v
WHERE v.bond_key_listed_market IN (SELECT bond_key FROM public.bond WHERE bond_id = '250001');

-- 6. Working comparison set: bonds that have BOTH a rating row and a valuation row.
SELECT b.bond_id, b.bond_subtype, b.rating_current, b.issuer_rating_current,
       v.valuation_date, v.valuation_net_price, v.valuation_yield, v.val_basis_point_value
FROM public.bond b
JOIN public.china_bond_valuation v
  ON v.bond_id = b.bond_id || '.' ||
     CASE b.mkt_type WHEN '1' THEN 'IB' WHEN '2' THEN 'IB' ELSE 'IB' END
WHERE b.bond_id IN ('269152', '282353', '282644')
ORDER BY b.bond_id;
