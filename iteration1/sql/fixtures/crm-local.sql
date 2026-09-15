-- 仅用于本机 ficc_ibor 测试库。字段来自提供的 CRM DDL，省略厂商 checksum 存储参数。
DO $$ BEGIN
    IF current_database() <> 'ficc_ibor' THEN
        RAISE EXCEPTION 'CRM fixture requires the local ficc_ibor database';
    END IF;
END $$;
CREATE TABLE IF NOT EXISTS public.crm_org (
    id varchar(32) PRIMARY KEY, org_name varchar(200) NOT NULL, org_short_name varchar(100),
    org_en_name varchar(200), org_en_short_name varchar(100), org_tel varchar(256), social_credit_code varchar(64),
    issue_date date, cancellation_date date, org_code varchar(64), org_category varchar(50), parent_org_id varchar(32),
    senpu_code varchar(32), senpu_parent_code varchar(32), org_type varchar(50), org_sub_type varchar(50),
    is_deregistered boolean DEFAULT false, deregistration_type varchar(50), org_level varchar(50), capital_type varchar(50),
    business_scope text, approval_date date, registered_capital numeric(20,2), paid_in_capital numeric(20,2),
    paid_in_ratio numeric(20,2), legal_person varchar(100), registered_country varchar(32), registered_province varchar(32),
    registered_city varchar(32), registered_district varchar(32), registered_address text, address text,
    office_country varchar(32), office_province varchar(32), office_city varchar(32), office_district varchar(32),
    office_address text, postal_code varchar(20), status smallint DEFAULT 1, tags text, trade_preference text, remark text,
    cfets_6_code varchar(32), cfets_21_code varchar(64), sse_code varchar(64), szse_trader_code varchar(64),
    szse_subject_code varchar(64), source_type varchar(64), source_table varchar(64), source_record_id varchar(100),
    sync_time timestamptz, deleted boolean DEFAULT false, created_by varchar(32), updated_by varchar(32),
    created_at timestamptz DEFAULT now(), updated_at timestamptz DEFAULT now(),
    org_name_manual boolean NOT NULL DEFAULT false, org_short_name_manual boolean NOT NULL DEFAULT false,
    org_name_py varchar(255), org_short_name_py varchar(255)
);
CREATE TABLE IF NOT EXISTS public.crm_contact (
    id varchar(32) PRIMARY KEY, name varchar(100) NOT NULL, tags text, status varchar(20) DEFAULT 'ACTIVE',
    position_name varchar(100), graduate_school varchar(200), birthday date, office_location varchar(200),
    qq varchar(64), ideal varchar(64), mobile varchar(64), phone varchar(64), email varchar(200), trade_preference text,
    remark text, cfets_trader_id varchar(64), cfets_trader_name varchar(100), source_type varchar(32), sync_time timestamptz,
    dept_id varchar(32), owner_user_ids text, deleted boolean DEFAULT false, created_by varchar(32), updated_by varchar(32),
    created_at timestamptz DEFAULT now(), updated_at timestamptz DEFAULT now(), company_email varchar(200),
    company_phone varchar(64), company_fax varchar(64), fax varchar(64), msn varchar(100), address text, postal_code varchar(20)
);
CREATE TABLE IF NOT EXISTS public.crm_contact_org_rel (
    id varchar(32) PRIMARY KEY, contact_id varchar(32) NOT NULL, org_id varchar(32) NOT NULL, dept_id varchar(32),
    position_name varchar(100), is_current boolean DEFAULT true, entry_date date, leave_date date,
    deleted boolean DEFAULT false, created_by varchar(32), updated_by varchar(32),
    created_at timestamptz DEFAULT now(), updated_at timestamptz DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_crm_org_name_id ON public.crm_org(org_name,id) WHERE deleted = false;
CREATE INDEX IF NOT EXISTS idx_crm_contact_name_active ON public.crm_contact(name) WHERE deleted = false;
CREATE INDEX IF NOT EXISTS idx_crm_contact_org_rel_contact_org_active ON public.crm_contact_org_rel(contact_id,org_id) WHERE deleted = false;
CREATE INDEX IF NOT EXISTS idx_crm_contact_org_rel_org_contact_active ON public.crm_contact_org_rel(org_id,contact_id) WHERE deleted = false;

-- 合成数据，不导入用户提供的联系人及联系方式。
INSERT INTO public.crm_org(id,org_name,org_short_name,org_name_py,org_short_name_py,deleted) VALUES
('9000000000000000001','测试甲银行股份有限公司','测试甲银行','ceshijiayinhang','csjyh',false),
('9000000000000000002','测试乙证券股份有限公司','测试乙证券','ceshiyizhengquan','csyzq',false),
('9000000000000000003','已删除测试机构','已删除测试机构','deleted','deleted',true)
ON CONFLICT (id) DO NOTHING;
INSERT INTO public.crm_contact(id,name,position_name,owner_user_ids,deleted) VALUES
('c9000000000000000001','测试张明','交易员','unrelated-owner',false),
('c9000000000000000002','测试张明','销售','another-owner',false),
('c9000000000000000003','已删除测试联系人',NULL,NULL,true),
('c9000000000000000004','测试离职联系人',NULL,NULL,false)
ON CONFLICT (id) DO NOTHING;
INSERT INTO public.crm_contact_org_rel(id,contact_id,org_id,is_current,deleted) VALUES
('r9001','c9000000000000000001','9000000000000000001',true,false),
('r9002','c9000000000000000001','9000000000000000002',true,false),
('r9003','c9000000000000000002','9000000000000000002',true,false),
('r9004','c9000000000000000003','9000000000000000001',true,false),
('r9005','c9000000000000000004','9000000000000000001',false,false),
('r9006','c9000000000000000002','9000000000000000001',true,true)
ON CONFLICT (id) DO NOTHING;
