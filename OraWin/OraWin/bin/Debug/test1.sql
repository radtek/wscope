

declare
  v_rowcount integer;
begin
  select count(*) into v_rowcount from dual
   where exists(select * from user_tables where table_name= upper('excelcontrol'));
  if v_rowcount = 0 then
    execute immediate'
        CREATE TABLE hs_user.excelcontrol
				(
              excel_name                    varchar2(100)            default '' ''    NOT NULL,
              excel_version                 varchar2(16)             default '' ''    NOT NULL
				)
				STORAGE(FREELISTS 20 FREELIST GROUPS 2)
				NOLOGGING
				tablespace HS_USER_DATA';
		execute immediate 'CREATE UNIQUE INDEX hs_user.idx_excelcontrol   ON hs_user.excelcontrol(excel_name)tablespace HS_USER_IDX';
		execute immediate 'DELETE hs_user.hsobjects WHERE object_name = ''excelcontrol'' and object_type = ''U''';
		execute immediate 'INSERT INTO hs_user.hsobjects (object_id, object_name, own_base, object_type, master_ver, second_ver, third_ver, build_ver)
    values(1043, ''excelcontrol'', ''HS_SYSTEM_DATA'', ''U'', ''6'', ''1'', ''4'', ''1'')';
  end if;
end;
/


declare
  v_rowcount integer;
begin
  select count(*) into v_rowcount from user_indexes where index_name =upper('idx_gemaccinfo_szstkacc');
  if v_rowcount = 0 then
    execute immediate 'CREATE   INDEX hs_user.idx_gemaccinfo_szstkacc  ON hs_user.gemaccinfo(sza_stkaccount)tablespace HS_USER_IDX ';
  end if;
end;
/

declare
  v_rowcount integer;
begin
  select count(*) into v_rowcount from dual
    where exists(select * from col
      where tname = upper('operators')
        and cname = upper('login_date'));
  if v_rowcount= 0 then
      execute immediate 'alter table operators add(login_date  NUMBER(10)  default 0 NOT NULL)';
  end if;
end;
/

declare
  v_rowcount integer;
begin
  select count (*) into v_rowcount from dual where exists(
    select * from col
      where tname = upper('licenseinfo')
        and cname = upper('license_product_id'));
  if v_rowcount = 0 then
    execute immediate 'alter table licenseinfo add(license_product_id  varchar2(10)      default ''00''    NOT NULL)';
  end if;
end;
/


declare
  v_rowcount integer;
begin
  select count(*) into v_rowcount from dual
   where exists(select * from user_tables where table_name= upper('noticereport'));
  if v_rowcount = 0 then
    execute immediate'
	CREATE TABLE hs_user.noticereport
	(
		operator_no                   varchar2(18)             default '' ''    NOT NULL,
		en_branch_no                  varchar2(2000)           default '' ''    NOT NULL,
		en_notice_type                varchar2(2000)           default '' ''    NOT NULL,
		target_ar                     varchar2(32)             default '' ''    NOT NULL,
		client_name                   varchar2(60)             default '' ''    NOT NULL,
		ar_staticpriority             NUMBER(10)               default 0	NOT NULL,
		report_status                 char(1)                  default '' ''    NOT NULL,
		client_version                NUMBER(10)               default 0	NOT NULL
	)
	STORAGE(FREELISTS 20 FREELIST GROUPS 2)
	NOLOGGING
	tablespace HS_USER_DATA';
    execute immediate 'CREATE UNIQUE INDEX hs_user.idx_noticereport ON hs_user.noticereport(client_name)tablespace HS_USER_IDX';
    execute immediate 'DELETE hs_user.hsobjects WHERE object_name = ''noticereport'' and object_type = ''U''';
    execute immediate 'INSERT INTO hs_user.hsobjects (object_id, object_name, own_base, object_type, master_ver, second_ver, third_ver, build_ver)
			values(1044, ''noticereport'', ''HS_SYSTEM_DATA'', ''U'', ''6'', ''1'', ''4'', ''1'')';
  end if;
end;
/

begin
  DELETE hsobjects WHERE object_name = 'PD_CUST_RISK_MODI' AND object_type = 'P';
  INSERT into hsobjects (
         object_id,          object_name,        own_base,           object_type,
         master_ver,         second_ver,         third_ver,          build_ver)
    values (
       150005,              'PD_CUST_RISK_MODI','USERDB',           'P',
         6,                  1,                  4,                  20120509);
end;
/

begin
  DELETE hsobjects WHERE object_name = 'PD_CUST_MODEL_COPY' AND object_type = 'P';
  INSERT into hsobjects (
         object_id,          object_name,        own_base,           object_type,
         master_ver,         second_ver,         third_ver,          build_ver)
    values (
       150006,              'PD_CUST_MODEL_COPY','USERDB',          'P',
         6,                  1,                  4,                  20111018);
end;
/

create or replace function hs_user.PD_CUST_MAIN_MODI
(
  p_op_branch_no                hstype.HsBranchNo%type,
  p_operator_no                 hstype.HsClientID%type,
  p_op_password                 hstype.HsPassword%type,
  p_op_station                  hstype.HsStation%type,
  p_op_entrust_way              hstype.HsType%type,
  p_function_id                 hstype.HsFunctionID%type,
  p_audit_action                hstype.HsType%type,
  p_action_in                   hstype.HsNumID%type,
  p_branch_no                   hstype.HsBranchNo%type,
  p_client_id                   hstype.HsClientID%type,
  p_client_card                 hstype.HsIDNO%type,
  p_client_name                 hstype.HsName%type,
  p_corp_client_group           hstype.HsClientGroup%type,
  p_client_sex                  hstype.HsType%type,
  p_nationality                 hstype.HsChar3%type,
  p_foreign_flag                hstype.HsFlag%type,
  p_id_kind                     hstype.HsType%type,
  p_id_no                       hstype.HsIdentity%type,
  p_id_begindate                hstype.HsDate%type,
  p_id_term                     hstype.HsNum%type,
  p_risk_level                  hstype.HsNum%type,
  p_init_date                   hstype.HsDate%type,
  p_business_flag               hstype.HsBusinessFlag%type,
  p_error_info             out  hstype.HsChar255%type,
  p_error_no               out  hstype.HsNum10%type,
  p_serial_no              out  hstype.HsSerialID%type,
  p_op_remark           in out  hstype.HsAbstract%type
)
return number
as
  v_modi_str                    hstype.HsChar16%type;
  v_position_str                hstype.HsChar32%type;
  v_client_card_b               hstype.HsIDNO%type;
  v_client_name_b               hstype.HsName%type;
  v_corp_client_group_b         hstype.HsClientGroup%type;
  v_client_sex_b                hstype.HsType%type;
  v_nationality_b               hstype.HsChar3%type;
  v_foreign_flag_b              hstype.HsFlag%type;
  v_id_kind_b                   hstype.HsType%type;
  v_id_no_b                     hstype.HsIdentity%type;
  v_id_begindate_b              hstype.HsDate%type;
  v_id_term_b                   hstype.HsNum%type;
  v_risk_level_b                hstype.HsNum%type;
  e_error                       exception;

begin
  p_error_info                  := ' ';
  p_error_no                    := 0;
  p_serial_no                   := 0;
  v_modi_str                    := ' ';
  v_position_str                := ' ';
  v_client_card_b               := ' ';
  v_client_name_b               := ' ';
  v_corp_client_group_b         := 0;
  v_client_sex_b                := ' ';
  v_nationality_b               := ' ';
  v_foreign_flag_b              := ' ';
  v_id_kind_b                   := ' ';
  v_id_no_b                     := ' ';
  v_id_begindate_b              := to_number(to_char(sysdate,'yyyymmdd'));
  v_id_term_b                   := 0;
  v_risk_level_b                := 0;

  p_error_no := PD_USER_SERIALNO_GET
  (
    p_branch_no                    => 0,
    p_serial_counter_no            => 10,
    p_error_info                   => p_error_info,
    p_error_no                     => p_error_no,
    p_serial_no                    => p_serial_no
  );
  if p_error_no != 0 then
    return(p_error_no);
  end if;
  v_position_str := p_init_date || right('0000000000' || p_serial_no, 10);
  v_modi_str := '000';

  begin
    select client_name,corp_client_group,client_sex,nationality,foreign_flag,id_kind,id_no,id_begindate,id_term,client_card,risk_level
     into v_client_name_b,v_corp_client_group_b,v_client_sex_b,v_nationality_b,v_foreign_flag_b,v_id_kind_b,v_id_no_b,v_id_begindate_b,v_id_term_b,v_client_card_b,v_risk_level_b
     from client
     where client_id = p_client_id;
  exception
     when others then
       p_error_no   := 150005;
       p_error_info := substrb('查询客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;

  if (p_action_in = 1 ) then
    if (p_corp_client_group != v_corp_client_group_b) then
      p_op_remark := p_op_remark || '公司客户分类 '||v_corp_client_group_b||'->'||p_corp_client_group;
    end if;

    if (p_client_sex != v_client_sex_b) then
      p_op_remark := p_op_remark || ',客户性别 '||v_client_sex_b||'->'||p_client_sex;
    end if;

    if (p_nationality != v_nationality_b) then
      p_op_remark := p_op_remark || ',国籍地区 '||v_nationality_b||'->'||p_nationality;
    end if;

    if (p_foreign_flag != v_foreign_flag_b) then
      p_op_remark := p_op_remark || ',境外标志 '||v_foreign_flag_b||'->'||p_foreign_flag;
    end if;

    if (p_client_card != v_client_card_b) then
      p_op_remark := p_op_remark || ',客户卡 '||v_client_card_b||'->'||p_client_card;
    end if;

    if (p_risk_level != v_risk_level_b) then
      p_op_remark := p_op_remark || ',风险级别 '||v_risk_level_b||'->'||p_risk_level;
    end if;
  end if;

  if (p_action_in = 2 ) then
    if (p_client_name != v_client_name_b) then
      p_op_remark := p_op_remark || '客户名称 '||v_client_name_b||'->'||p_client_name;
    end if;
  end if;

  if (p_action_in = 3 ) then
    if (p_id_kind != v_id_kind_b) then
      p_op_remark := p_op_remark || '证件类别 '||v_id_kind_b||'->'||p_id_kind;
    end if;

    if (p_id_no != v_id_no_b) then
      p_op_remark := p_op_remark || ',证件号码 '||v_id_no_b||'->'||p_id_no;
    end if;

  end if;

  if (p_action_in = 4) then
    if (p_id_no != v_id_no_b) then
      p_op_remark := p_op_remark || '证件号码 '||v_id_no_b||'->'||p_id_no;
    end if;

  end if;
  if (p_action_in = 5) then
    if (p_id_begindate != v_id_begindate_b) then
      p_op_remark := p_op_remark || ',证件开始日期 '||v_id_begindate_b||'->'||p_id_begindate;
    end if;

    if (p_id_term != v_id_term_b) then
      p_op_remark := p_op_remark || ',证件期限(指有效年份) '||v_id_term_b||'->'||p_id_term;
    end if;
  end if;
  if (p_action_in = 1 ) then
  begin
    update client
    set  corp_client_group = p_corp_client_group,
         client_sex = p_client_sex,
         nationality = p_nationality,
         foreign_flag = p_foreign_flag,
         client_card = p_client_card,
         risk_level = p_risk_level
     where client_id = p_client_id;
  exception
     when others then
       rollback;
       p_error_no   := 150003;
       p_error_info := substrb('更新客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;
  end if;

  if (p_action_in = 2 ) then
  begin
    update client
     set client_name = p_client_name
     where client_id = p_client_id;
  exception
     when others then
       rollback;
       p_error_no   := 150003;
       p_error_info := substrb('更新客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;
  end if;

  if (p_action_in = 3 ) then
  begin
    update client
    set  id_kind = p_id_kind,
         id_no = p_id_no
     where client_id = p_client_id;
  exception
     when others then
       rollback;
       p_error_no   := 150003;
       p_error_info := substrb('更新客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;
  end if;

  if (p_action_in = 4 ) then
  begin
    update client
     set id_no = p_id_no
     where client_id = p_client_id;
  exception
     when others then
       rollback;
       p_error_no   := 150003;
       p_error_info := substrb('更新客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;
  end if;
  if (p_action_in = 5 ) then
  begin
    update client
       set  id_begindate = p_id_begindate,
            id_term = p_id_term
     where client_id = p_client_id;
  exception
     when others then
       rollback;
       p_error_no   := 150003;
       p_error_info := substrb('更新客户基本信息表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       return(p_error_no);
  end;
  end if;

  begin
    insert into clientjour(
      init_date,          serial_no,          curr_date,          curr_time,
      business_flag,      op_branch_no,       operator_no,        op_station,
      client_id,          branch_no,          dev_branch_no,      client_name,
      corp_client_group,  asset_prop,         client_sex,         nationality,
      foreign_flag,       id_kind,            id_no,              id_begindate,
      id_term,            risk_level,         open_date,          cancel_date,
      client_status,      modi_str,           remark,             position_str,
      op_entrust_way,     corp_risk_level,    corp_begin_date,    corp_end_date)
    select
      p_init_date,        p_serial_no,        to_number(to_char(sysdate,'yyyymmdd')),to_number(to_char(sysdate,'hh24miss')),
      p_business_flag,    p_op_branch_no,     p_operator_no,      p_op_station,
      client_id,          branch_no,          dev_branch_no,      client_name,
      corp_client_group,  asset_prop,         client_sex,         nationality,
      foreign_flag,       id_kind,            id_no,              id_begindate,
      id_term,            risk_level,         open_date,          cancel_date,
      client_status,      v_modi_str,         p_op_remark,        v_position_str,
      p_op_entrust_way,   corp_risk_level,    corp_begin_date,    corp_end_date
    from client
    where client_id = p_client_id;
  exception
    when DUP_VAL_ON_INDEX then
      rollback;
      p_error_no   := 152004;
      p_error_info := substrb('客户基本信息流水已经存在[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);

      return (p_error_no);
    when e_error then
      rollback;
      p_error_no   := 150017;
      p_error_info := substrb('增加客户基本信息流水表[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);

      return (p_error_no);
  end;
  commit;
  return 0;
  return(0);
exception
  when others then
    rollback;
    p_error_no   := SQLCODE;
    p_error_info := SQLERRM;

    return(SQLCODE);
end PD_CUST_MAIN_MODI;
/

begin
  commit;
end;
/
