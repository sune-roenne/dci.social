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
  constraint pk_dcisoc_contest_round primary key(roundid),
  constraint fk_dcisoc_conestt_round_cont foreign key(contestid) references dcisoc_contest(contestid) on delete cascade
);
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




