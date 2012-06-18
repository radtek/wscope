create or replace function hs_user.PD_CUST_ACCO_OPEN
(
  p_op_branch_no                hstype.HsBranchNo%type,       -- 操作员分支代码
  p_operator_no                 hstype.HsClientID%type,       -- 操作员编号
  p_op_password                 hstype.HsPassword%type,       -- 操作员密码
  p_op_station                  hstype.HsStation%type,        -- 站点/电话
  p_op_entrust_way              hstype.HsType%type,           -- 委托方式
  p_function_id                 hstype.HsFunctionID%type,     -- 系统功能
  p_client_id                   hstype.HsClientID%type,       -- 客户编号
  p_branch_no                   hstype.HsBranchNo%type,       -- 分支代码
  p_dev_branch_no               hstype.HsBranchNo%type,       -- 开发分支代码
  p_corp_client_group           hstype.HsClientGroup%type,    -- 公司客户分类
  p_client_card                 hstype.HsIDNO%type,           -- 客户卡
  p_client_name                 hstype.HsName%type,           -- 客户姓名
  p_client_sex                  hstype.HsType%type,           -- 客户性别
  p_nationality                 hstype.HsChar3%type,          -- 国籍地区 20100630 麻理平 数据类型由HsType改为HsChar3
  p_asset_prop                  hstype.HsType%type,           -- 资产属性
  p_foreign_flag                hstype.HsFlag%type,           -- 境外标志
  p_id_kind                     hstype.HsType%type,           -- 身份类别
  p_id_no                       hstype.HsIdentity%type,       -- 证件号码
  p_id_begindate                hstype.HsDate%type,           -- 证件开始日期
  p_id_term                     hstype.HsNum%type,            -- 证件期限，指有效年份
  p_risk_level                  hstype.HsNum%type,            -- 风险级别
  p_birthday                    hstype.HsDate%type,           -- 出生日期
  p_last_name                   hstype.HsName2%type,          -- 投资人户名
  p_zipcode                     hstype.HsZip%type,            -- 邮政编码
  p_city_no                     hstype.HsChar4%type,          -- 城市编号
  p_home_tel                    hstype.HsPhone%type,          -- 住宅电话
  p_office_tel                  hstype.HsPhone%type,          -- 单位电话
  p_address                     hstype.HsAddress%type,        -- 联系地址
  p_id_address                  hstype.HsAddress%type,        -- 身份证地址 20100630 麻理平 数据类型由HsName2改为HsAddress
  p_phonecode                   hstype.HsPhone%type,          -- 联系电话
  p_contact_mobile              hstype.HsPhone%type,          -- 经办人手机
  p_e_mail                      hstype.HsChar64%type,         -- 电子信箱
  p_fax                         hstype.HsPhone%type,          -- 传真号码
  p_mobile_tel                  hstype.HsChar24%type,         -- 移动电话
  p_beeppager                   hstype.HsPhone%type,          -- 传呼号码
  p_mail_name                   hstype.HsName2%type,          -- 联系人
  p_relation_idtype             hstype.HsType%type,           -- 联系人证件类型
  p_relation_id                 hstype.HsIdentity%type,       -- 联系人证件号码
  p_instrepr_name               hstype.HsChar20%type,         -- 法人代表姓名
  p_instrepr_idtype             hstype.HsType%type,           -- 法人证件类型
  p_instrepr_id                 hstype.HsIdentity%type,       -- 法人证件号码
  p_degree_code                 hstype.HsType%type,           -- 学历代码
  p_profession_code             hstype.HsChar2%type,          -- 职业代码
  p_corporate_kind              hstype.HsType%type,           -- 企业类型
  p_income                      hstype.HsAmount%type,         -- 年收入
  p_child_flag                  hstype.HsFlag%type,           -- 未成年标志
  p_child_id                    hstype.HsIDENTITY%type,       -- 未成年人ID号
  p_statement_flag              hstype.HsType%type,           -- 对账单寄送选择 1. 不寄送2..按月3. 按季4.半年5.一年
  p_account_data                hstype.HsName2%type,          -- 开户规范信息 0  缺少身份证复印件  1  缺少证券账户卡复印件
  p_risk_info                   hstype.HsName2%type,          -- 风险要素信息 0 一对多户 1 休眠户 2 资料不齐户 3 授权代理户 4 二级户
  p_officeaddress               hstype.HsAddress%type,        -- 办公地址
  p_officezip                   hstype.HsZip%type,            -- 办公邮编
  p_nativeplace                 hstype.HsAddress%type,        -- 籍贯
  p_homeplace                   hstype.HsAddress%type,        -- 出生地
  p_icqid                       hstype.HsPhone%type,          -- ICQ地址
  p_roomhire                    hstype.HsAmount%type,         -- 房间租金
  p_sumhire                     hstype.HsAmount%type,         -- 累计租金
  p_specifycomputer             hstype.HsName2%type,          -- 指定电脑
  p_developer                   hstype.HsName%type,           -- 开发人员
  p_primcustmanager             hstype.HsName2%type,          -- 主办客户经理
  p_priminvestor                hstype.HsName2%type,          -- 主办投资顾问
  p_primsaleman                 hstype.HsName2%type,          -- 主办营销员
  p_primassetmanager            hstype.HsName2%type,          -- 主办资产管理经理
  p_remark                      hstype.HsAbstract%type,       -- 备注
  p_init_date                   hstype.HsDate%type,           -- 成交日期
  p_op_remark                   hstype.HsAbstract%type,       -- 操作备注
  p_client_status               hstype.HsStatus%type,         -- 客户状态
  p_organ_name                  hstype.HsName2%type,          -- 机构名称 20071106 毛荣 add
  p_company_name                hstype.HsName2%type,          -- 机构简称 20071106 毛荣 add
  p_organ_code                  hstype.HsChar32%type,         -- 组织机构代码 20071106 毛荣 add
  p_sale_licence                hstype.HsChar32%type,         -- 营业执照 20071106 毛荣 add
  p_tax_register                hstype.HsChar32%type,         -- 税务登记号码 20071106 毛荣 add
  p_company_kind                hstype.HsChar16%type,         -- 企业性质 20071106 毛荣 add
  p_work_range                  hstype.HsChar2000%type,       -- 经营范围 20071106 毛荣 add
  p_register_fund               hstype.HsAmount%type,         -- 注册资本 20071106 毛荣 add
  p_register_money_type         hstype.HsChar3%type,          -- 注册资本币种 20071106 毛荣 add
  p_contract_person             hstype.HsChar32%type,         -- 联系人 20071106 毛荣 add
  p_contract_tel                hstype.HsChar32%type,         -- 联系人电话 20071106 毛荣 add
  p_home_page                   hstype.HsChar64%type,         -- 网站地址 20071106 毛荣 add
  p_organ_flag                  hstype.HsType%type,           -- 机构标志（0个人1机构2自营户） 20071106 毛荣 add
  p_mainservorgan               hstype.HsName%type,           -- 总部服务机构 20080520 毛荣 add
  p_control_holder              hstype.HsName2%type,          -- 控股股东 20080715 毛荣 add
  p_instrepr_telephone          hstype.HsPhone%type,          -- 法定代表人电话号码 20080715 毛荣 add
  p_prove_kind                  hstype.HsType%type,           -- 证明文件类型 20080715 毛荣 add
  p_prove_id                    hstype.HsIDNo%type,           -- 证明文件号码 20080715 毛荣 add
  p_prove_period                hstype.HsDate%type,           -- 证明文件有效日期 20080715 毛荣 add
  p_hs_license                  hstype.HsChar255%type,        -- 授权密钥
  p_instbegindate               hstype.HsDate%type,           -- 法人证件起始日期
  p_instrepr_begin_date         hstype.HsDate%type,           -- 法人证件开始日
  p_instrepr_end_date           hstype.HsDate%type,           -- 法人证件到期日
  p_sale_begin_date             hstype.HsDate%type,           -- 营业执照起始日期
  p_sale_end_date               hstype.HsDate%type,           -- 营业执照有效截止日
  p_control_idtype              hstype.HsType%type,           -- 控股股东证件类型
  p_control_id                  hstype.HsChar32%type,         -- 控股股东证件号码
  p_control_begin_date          hstype.HsDate%type,           -- 控股股东证件开始日
  p_control_end_date            hstype.HsDate%type,           -- 控股股东证件到期日
  p_tax_begin_date              hstype.HsDate%type,           -- 税务登记开始日
  p_tax_end_date                hstype.HsDate%type,           -- 税务登记结束日
  p_relation_begin_date         hstype.HsDate%type,           -- 联系人证件开始日
  p_relation_end_date           hstype.HsDate%type,           -- 联系人证件到期日
  p_industry_type               hstype.HsChar2%type,          -- 行业信息
  p_open_date                   hstype.HsDate%type,           -- 开户日期 //20111108 贺渠瑛 add
  p_error_info             out  hstype.HsChar255%type,        -- 错误提示
  p_error_no               out  hstype.HsNum10%type,          -- 错误编号
  p_audit_serial_no        out  hstype.HsSerialID%type,       -- 复核流水号
  p_serial_no              out  hstype.HsSerialID%type,       -- 流水号
  p_business_flag       in out  hstype.HsBusinessFlag%type    -- 业务标志
)
return number
as 
  v_position_str                hstype.HsChar32%type;         -- 定位串
  v_modi_str                    hstype.HsChar16%type;         -- 修改标志位串
  v_serial_no1                  hstype.HsSerialID%type;       -- 流水号
  v_open_date_t                 hstype.HsDate%type;           -- 开户日期
  e_error                       exception;

begin
  p_error_info                  := ' ';
  p_error_no                    := 0;
  p_audit_serial_no             := 0;
  p_serial_no                   := 0;
  v_position_str                := ' ';
  v_modi_str                    := ' ';
  v_serial_no1                  := 0;
  v_open_date_t                 := to_number(to_char(sysdate,'yyyymmdd'));

  -- [过程_用户子系统流水号获取]
  p_error_no := PD_USER_SERIALNO_GET
  (
    p_branch_no                    => 0,
    p_serial_counter_no            => 10,
    p_error_info                   => p_error_info,                 -- out
    p_error_no                     => p_error_no,                   -- out
    p_serial_no                    => p_serial_no                   -- out
  );
  if p_error_no != 0 then
    return(p_error_no);
  end if;

  -- 事务处理开始
  -- 记录客户基本信息
  --20111108 贺渠瑛 增加如果传入的开户日期为0，则使用初始化日期作为开户日期
  if (p_open_date = 0) then
     v_open_date_t := p_init_date;
  else
     v_open_date_t := p_open_date;
  end if;
  v_position_str := lpad(p_branch_no,5,'0') || lpad(p_client_id, 18,'0')fadjl;
  begin
     --20111108 贺渠瑛 修改开户日期
     --[插入表记录][client][open_date=@init_date,cancel_date=0,corp_risk_level=0,corp_begin_date=0,corp_end_date=0]
     -- 插入表client记录
     insert into client(
       client_id,          branch_no,          dev_branch_no,      client_card,        
       client_name,        corp_client_group,  asset_prop,         client_sex,         
       nationality,        foreign_flag,       id_kind,            id_no,              
       id_begindate,       id_term,            risk_level,         open_date,          
       cancel_date,        client_status,      position_str,       corp_risk_level,    
       corp_begin_date,    corp_end_date)
     values (
       p_client_id,        p_branch_no,        p_dev_branch_no,    p_client_card,      
       p_client_name,      p_corp_client_group,p_asset_prop,       p_client_sex,       
       p_nationality,      p_foreign_flag,     p_id_kind,          p_id_no,            
       p_id_begindate,     p_id_term,          p_risk_level,       v_open_date_t,      
       0,                  p_client_status,    v_position_str,     0,                  
       0,                  0);
  exception
     when e_error then
       rollback;
       p_error_no   := 150002;
       p_error_info := substrb('增加客户基本信息失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;
  --20071106 毛荣 add 支持机构客户信息表的记录 begin
  if ((trim(p_organ_flag) is not null) and (p_organ_flag != '0')) then
    -- 记录机构客户信息
    --20100524 俱锁利 插入表organinfo去掉remark=@op_remark 
    begin
       --[插入表记录][organinfo][remark=@op_remark]
       --20110321 chenjf organ_begin_date=@id_begindate,organ_end_date=@id_term
       -- 插入表organinfo记录
       insert into organinfo(
         client_id,          branch_no,          organ_name,         company_name,       
         instrepr_name,      organ_code,         sale_licence,       tax_register,       
         company_kind,       work_range,         register_fund,      register_money_type,
         contract_person,    contact_mobile,     relation_idtype,    relation_id,        
         contract_tel,       fax,                e_mail,             home_page,          
         nationality,        address,            zipcode,            remark,             
         position_str,       control_holder,     instrepr_telephone, prove_kind,         
         prove_id,           prove_period,       instrepr_idtype,    instrepr_id,        
         instrepr_begin_date,instrepr_end_date,  sale_begin_date,    sale_end_date,      
         tax_begin_date,     tax_end_date,       control_idtype,     control_id,         
         control_begin_date, control_end_date,   relation_begin_date,relation_end_date,  
         organ_begin_date,   organ_end_date)
       values (
         p_client_id,        p_branch_no,        p_organ_name,       p_company_name,     
         p_instrepr_name,    p_organ_code,       p_sale_licence,     p_tax_register,     
         p_company_kind,     p_work_range,       p_register_fund,    p_register_money_type,
         p_contract_person,  p_contact_mobile,   p_relation_idtype,  p_relation_id,      
         p_contract_tel,     p_fax,              p_e_mail,           p_home_page,        
         p_nationality,      p_address,          p_zipcode,          p_remark,           
         v_position_str,     p_control_holder,   p_instrepr_telephone,p_prove_kind,       
         p_prove_id,         p_prove_period,     p_instrepr_idtype,  p_instrepr_id,      
         p_instrepr_begin_date,p_instrepr_end_date,p_sale_begin_date,  p_sale_end_date,    
         p_tax_begin_date,   p_tax_end_date,     p_control_idtype,   p_control_id,       
         p_control_begin_date,p_control_end_date, p_relation_begin_date,p_relation_end_date,
         p_id_begindate,     p_id_term);
    exception
       when e_error then
         rollback;
         p_error_no   := 150032;
         p_error_info := substrb('增加机构客户信息失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
         
         return (p_error_no);
    end;
  end if;
  --20071106 毛荣 add 支持机构客户信息表的记录 end

  -- 记录客户流水
  v_position_str := p_init_date || right('0000000000' || p_serial_no, 10);
  v_modi_str := '111';
  -- 记录客户基本信息流水
  begin
     -- 插入表clientjour记录
     insert into clientjour(
       init_date,          serial_no,          curr_date,          curr_time,          
       business_flag,      op_branch_no,       operator_no,        op_station,         
       client_id,          branch_no,          dev_branch_no,      client_name,        
       corp_client_group,  asset_prop,         client_sex,         nationality,        
       foreign_flag,       id_kind,            id_no,              id_begindate,       
       id_term,            risk_level,         open_date,          cancel_date,        
       client_status,      modi_str,           remark,             position_str,       
       op_entrust_way,     corp_risk_level,    corp_begin_date,    corp_end_date)
     values (
       p_init_date,        p_serial_no,        to_number(to_char(sysdate,'yyyymmdd')),to_number(to_char(sysdate,'hh24miss')),
       p_business_flag,    p_op_branch_no,     p_operator_no,      p_op_station,       
       p_client_id,        p_branch_no,        p_dev_branch_no,    p_client_name,      
       p_corp_client_group,p_asset_prop,       p_client_sex,       p_nationality,      
       p_foreign_flag,     p_id_kind,          p_id_no,            p_id_begindate,     
       p_id_term,          p_risk_level,       p_open_date,        0,                  
       p_client_status,    v_modi_str,         p_op_remark,        v_position_str,     
       p_op_entrust_way,   0,                  0,                  0);
  exception
     when e_error then
       rollback;
       p_error_no   := 150017;
       p_error_info := substrb('增加客户基本信息流水表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  -- 记录客户其它信息
  --20100524 俱锁利 插入表clientinfo去掉remark=@op_remark 
  begin
     --[插入表记录][clientinfo][remark=@op_remark]
     -- 插入表clientinfo记录
     insert into clientinfo(
       client_id,          branch_no,          birthday,           last_name,          
       confirm_date,       zipcode,            city_no,            home_tel,           
       office_tel,         address,            id_address,         phonecode,          
       contact_mobile,     e_mail,             fax,                mobile_tel,         
       beeppager,          mail_name,          relation_idtype,    relation_id,        
       instrepr_idtype,    instrepr_id,        instrepr_name,      degree_code,        
       profession_code,    corporate_kind,     income,             child_flag,         
       child_id,           statement_flag,     account_data,       risk_info,          
       officeaddress,      officezip,          nativeplace,        homeplace,          
       icqid,              roomhire,           sumhire,            specifycomputer,    
       primcustmanager,    developer,          mainservorgan,      priminvestor,       
       primsaleman,        primassetmanager,   remark,             industry_type)
     values (
       p_client_id,        p_branch_no,        p_birthday,         p_last_name,        
       to_number(to_char(sysdate,'yyyymmdd')),p_zipcode,          p_city_no,          p_home_tel,         
       p_office_tel,       p_address,          p_id_address,       p_phonecode,        
       p_contact_mobile,   p_e_mail,           p_fax,              p_mobile_tel,       
       p_beeppager,        p_mail_name,        p_relation_idtype,  p_relation_id,      
       p_instrepr_idtype,  p_instrepr_id,      p_instrepr_name,    p_degree_code,      
       p_profession_code,  p_corporate_kind,   p_income,           p_child_flag,       
       p_child_id,         p_statement_flag,   p_account_data,     p_risk_info,        
       p_officeaddress,    p_officezip,        p_nativeplace,      p_homeplace,        
       p_icqid,            p_roomhire,         p_sumhire,          p_specifycomputer,  
       p_primcustmanager,  p_developer,        p_mainservorgan,    p_priminvestor,     
       p_primsaleman,      p_primassetmanager, p_remark,           p_industry_type);
  exception
     when e_error then
       rollback;
       p_error_no   := 150007;
       p_error_info := substrb('增加客户其他信息失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  -- 记录客户其它信息流水
  begin
     -- 插入表clientinfojour记录
     insert into clientinfojour(
       init_date,          serial_no,          curr_date,          curr_time,          
       client_id,          branch_no,          op_branch_no,       operator_no,        
       op_station,         birthday,           last_name,          confirm_date,       
       zipcode,            city_no,            home_tel,           office_tel,         
       address,            id_address,         phonecode,          contact_mobile,     
       e_mail,             fax,                mobile_tel,         beeppager,          
       mail_name,          relation_idtype,    relation_id,        instrepr_idtype,    
       instrepr_id,        instrepr_name,      degree_code,        profession_code,    
       corporate_kind,     income,             child_flag,         child_id,           
       statement_flag,     account_data,       risk_info,          officeaddress,      
       officezip,          nativeplace,        homeplace,          icqid,              
       romeid,             romehire,           sumhire,            specifycomputer,    
       developer,          mainservorgan,      primcustmanager,    priminvestor,       
       primsaleman,        primassetmanager,   remark,             position_str,       
       op_entrust_way,     industry_type)
     values (
       p_init_date,        p_serial_no,        to_number(to_char(sysdate,'yyyymmdd')),to_number(to_char(sysdate,'hh24miss')),
       p_client_id,        p_branch_no,        p_op_branch_no,     p_operator_no,      
       p_op_station,       p_birthday,         p_last_name,        to_number(to_char(sysdate,'yyyymmdd')),
       p_zipcode,          p_city_no,          p_home_tel,         p_office_tel,       
       p_address,          p_id_address,       p_phonecode,        p_contact_mobile,   
       p_e_mail,           p_fax,              p_mobile_tel,       p_beeppager,        
       p_mail_name,        p_relation_idtype,  p_relation_id,      p_instrepr_idtype,  
       p_instrepr_id,      p_instrepr_name,    p_degree_code,      p_profession_code,  
       p_corporate_kind,   p_income,           p_child_flag,       p_child_id,         
       p_statement_flag,   p_account_data,     p_risk_info,        p_officeaddress,    
       p_officezip,        p_nativeplace,      p_homeplace,        p_icqid,            
       0,                  0.0,                p_sumhire,          p_specifycomputer,  
       p_developer,        p_mainservorgan,    p_primcustmanager,  p_priminvestor,     
       p_primsaleman,      p_primassetmanager, p_op_remark,        v_position_str,     
       p_op_entrust_way,   p_industry_type);
  exception
     when e_error then
       rollback;
       p_error_no   := 150012;
       p_error_info := substrb('增加客户其他信息流水表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  --20071106 毛荣 add 支持机构客户信息表的记录 begin
  if ((trim(p_organ_flag) is not null) and (p_organ_flag != '0')) then
    -- 记录机构客户信息流水
    begin
       --20110321 chenjf organ_begin_date=@id_begindate,organ_end_date=@id_term
       -- 插入表organinfojour记录
       insert into organinfojour(
         init_date,          serial_no,          curr_date,          curr_time,          
         op_branch_no,       operator_no,        op_station,         client_id,          
         branch_no,          organ_name,         company_name,       instrepr_name,      
         organ_code,         sale_licence,       tax_register,       company_kind,       
         work_range,         register_fund,      register_money_type,contract_person,    
         contact_mobile,     relation_idtype,    relation_id,        contract_tel,       
         fax,                e_mail,             home_page,          nationality,        
         address,            zipcode,            remark,             position_str,       
         control_holder,     instrepr_telephone, prove_kind,         prove_id,           
         prove_period,       instrepr_idtype,    instrepr_id,        instrepr_begin_date,
         instrepr_end_date,  sale_begin_date,    sale_end_date,      tax_begin_date,     
         tax_end_date,       control_idtype,     control_id,         control_begin_date, 
         control_end_date,   relation_begin_date,relation_end_date,  organ_begin_date,   
         organ_end_date)
       values (
         p_init_date,        p_serial_no,        to_number(to_char(sysdate,'yyyymmdd')),to_number(to_char(sysdate,'hh24miss')),
         p_op_branch_no,     p_operator_no,      p_op_station,       p_client_id,        
         p_branch_no,        p_organ_name,       p_company_name,     p_instrepr_name,    
         p_organ_code,       p_sale_licence,     p_tax_register,     p_company_kind,     
         p_work_range,       p_register_fund,    p_register_money_type,p_contract_person,  
         p_contact_mobile,   p_relation_idtype,  p_relation_id,      p_contract_tel,     
         p_fax,              p_e_mail,           p_home_page,        p_nationality,      
         p_address,          p_zipcode,          p_op_remark,        v_position_str,     
         p_control_holder,   p_instrepr_telephone,p_prove_kind,       p_prove_id,         
         p_prove_period,     p_instrepr_idtype,  p_instrepr_id,      p_instrepr_begin_date,
         p_instrepr_end_date,p_sale_begin_date,  p_sale_end_date,    p_tax_begin_date,   
         p_tax_end_date,     p_control_idtype,   p_control_id,       p_control_begin_date,
         p_control_end_date, p_relation_begin_date,p_relation_end_date,p_id_begindate,     
         p_id_term);
    exception
       when e_error then
         rollback;
         p_error_no   := 150033;
         p_error_info := substrb('增加机构客户信息流水表失败[相关参数：p_client_id = ' || p_client_id || ']', 1, 255);
         
         return (p_error_no);
    end;
  end if;
  --20071106 毛荣 add 支持机构客户信息表的记录 end
  commit;
  return 0;
  return(0);
exception
  when others then
    rollback;
    p_error_no   := SQLCODE;
    p_error_info := SQLERRM;

    return(SQLCODE);
end PD_CUST_ACCO_OPEN;

commit;
