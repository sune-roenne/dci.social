create table dcisoc_contest (
  contestid number(10) generated always as identity not null enable,
  contestname varchar2(200) not null enable,
  constraint pk_dcisoc_contest primary key(contestid)
);

create unique index idx_dcisoc_contest_nam on dcisoc_contest(contestname);

create table dcisoc_contest_round (
  roundid number(10) generated always as identity not null enable,
  contestid number(10) not null enable,
  roundindex number(5) not null enable,
  roundname varchar2(200) not null enable,
  roundtimeinseconds number(5) not null enable,
  pointsnominal number(5) not null enable,
  roundtype varchar2(100) not null enable,
  question varchar2(2000),
  soundid varchar2(200),
  answeroption number(10),
  additionalseconds number(5) default 0 not null enable,
  constraint pk_dcisoc_contest_round primary key(roundid),
  constraint fk_dcisoc_conestt_round_cont foreign key(contestid) references dcisoc_contest(contestid) on delete cascade
);


;
create table dcisoc_sound (
  soundid varchar2(200) not null enable,
  soundname varchar2(200) not null enable,
  soundbytes blob not null enable,
  hashvalue varchar2(1000) not null enable,
  durationinseconds number(5) not null enable,
  constraint pk_dcisoc_sound primary key (soundid)
);

create index idx_dcisoc_sound_hash on dcisoc_sound(hashvalue);

;
--drop table dcisoc_contest_round
create index idx_dcisoc_contest_round_cont on dcisoc_contest_round(contestid);

create table dcisoc_contest_round_option (
  roundoptionid number(10) generated always as identity not null enable,
  roundid number(10) not null enable,
  optionindex number(5) not null enable,
  optionname varchar2(200) not null enable,
  constraint pk_dcisoc_contest_round_option primary key(roundoptionid),
  constraint fk_dcisoc_contest_round_option_rnd foreign key(roundid) references dcisoc_contest_round(roundid) on delete cascade
);

create unique index idx_dcisoc_contest_round_option_nam on dcisoc_contest_round_option(optionname);
create unique index idx_dcisoc_contest_round_option_idx on dcisoc_contest_round_option(roundid, optionindex);


/*
#######################################################################################
############################# EXECUTION################################################
#######################################################################################
*/


create table dcisoc_contex (
  executionid number(10) generated always as identity not null enable,
  contestid number(10) not null,
  contestname varchar2(200) not null enable,
  starttime timestamp not null enable,
  endtime timestamp,
  constraint pk_dcisoc_contex primary key(executionid)
);
--drop table dcisoc_contex


;
create index idx_dcisoc_contex_cont on dcisoc_contex(contestid);

create table dcisoc_contex_round (
  roundexecutionid number(10) generated always as identity not null enable,
  executionid number(10) not null enable,
  roundid number(10) not null enable,
  roundname varchar2(200) not null enable,
  starttime timestamp not null enable,
  endtime timestamp,
  answeroption number(10),
  constraint pk_dcisoc_contex_round primary key(roundexecutionid),
  constraint fk_dcisoc_contex_round_exec foreign key (executionid) references dcisoc_contex(executionid) on delete cascade
);

create index idx_dcisoc_contex_round_exec on dcisoc_contex_round(executionid);
create index idx_dcisoc_contex_round_rnd on dcisoc_contex_round(roundid);


create table dcisoc_contex_round_select (
  roundexecutionid number(10) generated always as identity not null enable,
  userid number(10) not null enable,
  roundoptionid number(10) not null enable,
  roundoptionvalue varchar2(200) not null enable,
  selecttime timestamp not null enable,
  iscorrect number(1) not null enable,
  constraint pk_dcisoc_contex_round_select primary key(roundexecutionid, userid)
);

create table dcisoc_contex_round_buzz (
  roundexecutionid number(10) generated always as identity not null enable,
  userid number(10) not null enable,
  buzztime timestamp not null enable,
  iscorrect number(1) not null enable,
  constraint pk_dcisoc_contex_round_buzz primary key(roundexecutionid, userid)
);

--drop table dcisoc_contex_round;



