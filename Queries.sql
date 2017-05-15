SELECT TABLE_NAME FROM DWIZ.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

select * from Account;
select * from Jog where Id = 947;
select * from Jog order by Date desc;
select * from ServiceLog order by UtcTime desc;

/*
update Account set Coach = 'foo' where UserName = 'dude3';

delete from Account;
delete from Jog;
delete from ServiceLog;

drop table Account;
drop table Jog;
drop table ServiceLog;
drop table __EFMigrationsHistory;
*/