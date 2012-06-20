-- tst
/* women */
declare  v_rowcount integer; 

   --- sjfalsdfja
begin  select count(*) into v_rowcount /* womendajia */ from 
dual where /* weororq
       fadsfa
          djfdla*/
exists(    select * from /*+ womedaji */ col    /* faldsjfa 
test /* good well */
 where tname = upper('afofetfcode')       and cname = upper('init_date'));  
  /*nihao*/ /*jestlj*/  /*jlwjelf*/
	if v_rowcount = 0 Then    -- test
	execute immediate 'alter table afofetfcode add init_date NUMBER(10) default to_number(to_char(sysdate,''yyyymmdd'')) NOT NULL';
end if; 
end;