-- Project Name : dfsample
-- Date/Time    : 2014/02/07 19:22:08
-- Author       : dev
-- RDBMS Type   : PostgreSQL
-- Application  : A5:SQL Mk-2

-- 
create table my_table (
  id serial not null
  , user_name character varying(100)
  , age integer not null
  , attendance_flag character(1) not null
  , created_datetime timestamp not null
  , created_user character varying(100) not null
  , updated_datetime timestamp
  , updated_user character varying(100)
  , version_no integer not null
  , constraint my_table_PKC primary key (id)
) ;

comment on column my_table.id is 'id';
comment on column my_table.user_name is 'ユーザ名';
comment on column my_table.age is '年齢';
comment on column my_table.attendance_flag is '出欠フラグ';
comment on column my_table.created_datetime is '作成日時';
comment on column my_table.created_user is '作成ユーザ';
comment on column my_table.updated_datetime is '更新日時';
comment on column my_table.updated_user is '更新ユーザ';
comment on column my_table.version_no is '排他制御用項目';
