create or replace function hs_user.PD_CUST_ACCO_OPEN
(
  p_op_branch_no                hstype.HsBranchNo%type,       -- ����Ա��֧����
  p_operator_no                 hstype.HsClientID%type,       -- ����Ա���
  p_op_password                 hstype.HsPassword%type,       -- ����Ա����
  p_op_station                  hstype.HsStation%type,        -- վ��/�绰
  p_op_entrust_way              hstype.HsType%type,           -- ί�з�ʽ
  p_function_id                 hstype.HsFunctionID%type,     -- ϵͳ����
  p_client_id                   hstype.HsClientID%type,       -- �ͻ����
  p_branch_no                   hstype.HsBranchNo%type,       -- ��֧����
  p_dev_branch_no               hstype.HsBranchNo%type,       -- ������֧����
  p_corp_client_group           hstype.HsClientGroup%type,    -- ��˾�ͻ�����
  p_client_card                 hstype.HsIDNO%type,           -- �ͻ���
  p_client_name                 hstype.HsName%type,           -- �ͻ�����
  p_client_sex                  hstype.HsType%type,           -- �ͻ��Ա�
  p_nationality                 hstype.HsChar3%type,          -- �������� 20100630 ����ƽ ����������HsType��ΪHsChar3
  p_asset_prop                  hstype.HsType%type,           -- �ʲ�����
  p_foreign_flag                hstype.HsFlag%type,           -- �����־
  p_id_kind                     hstype.HsType%type,           -- ������
  p_id_no                       hstype.HsIdentity%type,       -- ֤������
  p_id_begindate                hstype.HsDate%type,           -- ֤����ʼ����
  p_id_term                     hstype.HsNum%type,            -- ֤�����ޣ�ָ��Ч���
  p_risk_level                  hstype.HsNum%type,            -- ���ռ���
  p_birthday                    hstype.HsDate%type,           -- ��������
  p_last_name                   hstype.HsName2%type,          -- Ͷ���˻���
  p_zipcode                     hstype.HsZip%type,            -- ��������
  p_city_no                     hstype.HsChar4%type,          -- ���б��
  p_home_tel                    hstype.HsPhone%type,          -- סլ�绰
  p_office_tel                  hstype.HsPhone%type,          -- ��λ�绰
  p_address                     hstype.HsAddress%type,        -- ��ϵ��ַ
  p_id_address                  hstype.HsAddress%type,        -- ���֤��ַ 20100630 ����ƽ ����������HsName2��ΪHsAddress
  p_phonecode                   hstype.HsPhone%type,          -- ��ϵ�绰
  p_contact_mobile              hstype.HsPhone%type,          -- �������ֻ�
  p_e_mail                      hstype.HsChar64%type,         -- ��������
  p_fax                         hstype.HsPhone%type,          -- �������
  p_mobile_tel                  hstype.HsChar24%type,         -- �ƶ��绰
  p_beeppager                   hstype.HsPhone%type,          -- ��������
  p_mail_name                   hstype.HsName2%type,          -- ��ϵ��
  p_relation_idtype             hstype.HsType%type,           -- ��ϵ��֤������
  p_relation_id                 hstype.HsIdentity%type,       -- ��ϵ��֤������
  p_instrepr_name               hstype.HsChar20%type,         -- ���˴�������
  p_instrepr_idtype             hstype.HsType%type,           -- ����֤������
  p_instrepr_id                 hstype.HsIdentity%type,       -- ����֤������
  p_degree_code                 hstype.HsType%type,           -- ѧ������
  p_profession_code             hstype.HsChar2%type,          -- ְҵ����
  p_corporate_kind              hstype.HsType%type,           -- ��ҵ����
  p_income                      hstype.HsAmount%type,         -- ������
  p_child_flag                  hstype.HsFlag%type,           -- δ�����־
  p_child_id                    hstype.HsIDENTITY%type,       -- δ������ID��
  p_statement_flag              hstype.HsType%type,           -- ���˵�����ѡ�� 1. ������2..����3. ����4.����5.һ��
  p_account_data                hstype.HsName2%type,          -- �����淶��Ϣ 0  ȱ�����֤��ӡ��  1  ȱ��֤ȯ�˻�����ӡ��
  p_risk_info                   hstype.HsName2%type,          -- ����Ҫ����Ϣ 0 һ�Զ໧ 1 ���߻� 2 ���ϲ��뻧 3 ��Ȩ���� 4 ������
  p_officeaddress               hstype.HsAddress%type,        -- �칫��ַ
  p_officezip                   hstype.HsZip%type,            -- �칫�ʱ�
  p_nativeplace                 hstype.HsAddress%type,        -- ����
  p_homeplace                   hstype.HsAddress%type,        -- ������
  p_icqid                       hstype.HsPhone%type,          -- ICQ��ַ
  p_roomhire                    hstype.HsAmount%type,         -- �������
  p_sumhire                     hstype.HsAmount%type,         -- �ۼ����
  p_specifycomputer             hstype.HsName2%type,          -- ָ������
  p_developer                   hstype.HsName%type,           -- ������Ա
  p_primcustmanager             hstype.HsName2%type,          -- ����ͻ�����
  p_priminvestor                hstype.HsName2%type,          -- ����Ͷ�ʹ���
  p_primsaleman                 hstype.HsName2%type,          -- ����Ӫ��Ա
  p_primassetmanager            hstype.HsName2%type,          -- �����ʲ�������
  p_remark                      hstype.HsAbstract%type,       -- ��ע
  p_init_date                   hstype.HsDate%type,           -- �ɽ�����
  p_op_remark                   hstype.HsAbstract%type,       -- ������ע
  p_client_status               hstype.HsStatus%type,         -- �ͻ�״̬
  p_organ_name                  hstype.HsName2%type,          -- �������� 20071106 ë�� add
  p_company_name                hstype.HsName2%type,          -- ������� 20071106 ë�� add
  p_organ_code                  hstype.HsChar32%type,         -- ��֯�������� 20071106 ë�� add
  p_sale_licence                hstype.HsChar32%type,         -- Ӫҵִ�� 20071106 ë�� add
  p_tax_register                hstype.HsChar32%type,         -- ˰��ǼǺ��� 20071106 ë�� add
  p_company_kind                hstype.HsChar16%type,         -- ��ҵ���� 20071106 ë�� add
  p_work_range                  hstype.HsChar2000%type,       -- ��Ӫ��Χ 20071106 ë�� add
  p_register_fund               hstype.HsAmount%type,         -- ע���ʱ� 20071106 ë�� add
  p_register_money_type         hstype.HsChar3%type,          -- ע���ʱ����� 20071106 ë�� add
  p_contract_person             hstype.HsChar32%type,         -- ��ϵ�� 20071106 ë�� add
  p_contract_tel                hstype.HsChar32%type,         -- ��ϵ�˵绰 20071106 ë�� add
  p_home_page                   hstype.HsChar64%type,         -- ��վ��ַ 20071106 ë�� add
  p_organ_flag                  hstype.HsType%type,           -- ������־��0����1����2��Ӫ���� 20071106 ë�� add
  p_mainservorgan               hstype.HsName%type,           -- �ܲ�������� 20080520 ë�� add
  p_control_holder              hstype.HsName2%type,          -- �عɹɶ� 20080715 ë�� add
  p_instrepr_telephone          hstype.HsPhone%type,          -- ���������˵绰���� 20080715 ë�� add
  p_prove_kind                  hstype.HsType%type,           -- ֤���ļ����� 20080715 ë�� add
  p_prove_id                    hstype.HsIDNo%type,           -- ֤���ļ����� 20080715 ë�� add
  p_prove_period                hstype.HsDate%type,           -- ֤���ļ���Ч���� 20080715 ë�� add
  p_hs_license                  hstype.HsChar255%type,        -- ��Ȩ��Կ
  p_instbegindate               hstype.HsDate%type,           -- ����֤����ʼ����
  p_instrepr_begin_date         hstype.HsDate%type,           -- ����֤����ʼ��
  p_instrepr_end_date           hstype.HsDate%type,           -- ����֤��������
  p_sale_begin_date             hstype.HsDate%type,           -- Ӫҵִ����ʼ����
  p_sale_end_date               hstype.HsDate%type,           -- Ӫҵִ����Ч��ֹ��
  p_control_idtype              hstype.HsType%type,           -- �عɹɶ�֤������
  p_control_id                  hstype.HsChar32%type,         -- �عɹɶ�֤������
  p_control_begin_date          hstype.HsDate%type,           -- �عɹɶ�֤����ʼ��
  p_control_end_date            hstype.HsDate%type,           -- �عɹɶ�֤��������
  p_tax_begin_date              hstype.HsDate%type,           -- ˰��Ǽǿ�ʼ��
  p_tax_end_date                hstype.HsDate%type,           -- ˰��Ǽǽ�����
  p_relation_begin_date         hstype.HsDate%type,           -- ��ϵ��֤����ʼ��
  p_relation_end_date           hstype.HsDate%type,           -- ��ϵ��֤��������
  p_industry_type               hstype.HsChar2%type,          -- ��ҵ��Ϣ
  p_open_date                   hstype.HsDate%type,           -- �������� //20111108 ������ add
  p_error_info             out  hstype.HsChar255%type,        -- ������ʾ
  p_error_no               out  hstype.HsNum10%type,          -- ������
  p_audit_serial_no        out  hstype.HsSerialID%type,       -- ������ˮ��
  p_serial_no              out  hstype.HsSerialID%type,       -- ��ˮ��
  p_business_flag       in out  hstype.HsBusinessFlag%type    -- ҵ���־
)
return number
as 
  v_position_str                hstype.HsChar32%type;         -- ��λ��
  v_modi_str                    hstype.HsChar16%type;         -- �޸ı�־λ��
  v_serial_no1                  hstype.HsSerialID%type;       -- ��ˮ��
  v_open_date_t                 hstype.HsDate%type;           -- ��������
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

  -- [����_�û���ϵͳ��ˮ�Ż�ȡ]
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

  -- ������ʼ
  -- ��¼�ͻ�������Ϣ
  --20111108 ������ �����������Ŀ�������Ϊ0����ʹ�ó�ʼ��������Ϊ��������
  if (p_open_date = 0) then
     v_open_date_t := p_init_date;
  else
     v_open_date_t := p_open_date;
  end if;
  v_position_str := lpad(p_branch_no,5,'0') || lpad(p_client_id, 18,'0')fadjl;
  begin
     --20111108 ������ �޸Ŀ�������
     --[������¼][client][open_date=@init_date,cancel_date=0,corp_risk_level=0,corp_begin_date=0,corp_end_date=0]
     -- �����client��¼
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
       p_error_info := substrb('���ӿͻ�������Ϣʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;
  --20071106 ë�� add ֧�ֻ����ͻ���Ϣ��ļ�¼ begin
  if ((trim(p_organ_flag) is not null) and (p_organ_flag != '0')) then
    -- ��¼�����ͻ���Ϣ
    --20100524 ������ �����organinfoȥ��remark=@op_remark 
    begin
       --[������¼][organinfo][remark=@op_remark]
       --20110321 chenjf organ_begin_date=@id_begindate,organ_end_date=@id_term
       -- �����organinfo��¼
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
         p_error_info := substrb('���ӻ����ͻ���Ϣʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
         
         return (p_error_no);
    end;
  end if;
  --20071106 ë�� add ֧�ֻ����ͻ���Ϣ��ļ�¼ end

  -- ��¼�ͻ���ˮ
  v_position_str := p_init_date || right('0000000000' || p_serial_no, 10);
  v_modi_str := '111';
  -- ��¼�ͻ�������Ϣ��ˮ
  begin
     -- �����clientjour��¼
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
       p_error_info := substrb('���ӿͻ�������Ϣ��ˮ��ʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  -- ��¼�ͻ�������Ϣ
  --20100524 ������ �����clientinfoȥ��remark=@op_remark 
  begin
     --[������¼][clientinfo][remark=@op_remark]
     -- �����clientinfo��¼
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
       p_error_info := substrb('���ӿͻ�������Ϣʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  -- ��¼�ͻ�������Ϣ��ˮ
  begin
     -- �����clientinfojour��¼
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
       p_error_info := substrb('���ӿͻ�������Ϣ��ˮ��ʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
       
       return (p_error_no);
  end;

  --20071106 ë�� add ֧�ֻ����ͻ���Ϣ��ļ�¼ begin
  if ((trim(p_organ_flag) is not null) and (p_organ_flag != '0')) then
    -- ��¼�����ͻ���Ϣ��ˮ
    begin
       --20110321 chenjf organ_begin_date=@id_begindate,organ_end_date=@id_term
       -- �����organinfojour��¼
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
         p_error_info := substrb('���ӻ����ͻ���Ϣ��ˮ��ʧ��[��ز�����p_client_id = ' || p_client_id || ']', 1, 255);
         
         return (p_error_no);
    end;
  end if;
  --20071106 ë�� add ֧�ֻ����ͻ���Ϣ��ļ�¼ end
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
