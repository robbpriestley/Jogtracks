SELECT TABLE_NAME FROM DWIZ.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

/*
select * from Account;
select * from Item;
select * from ServiceLog order by UtcTime desc;

update Account set Coach = 'foo' where Id = 6;

delete from Account;
delete from Item;
delete from ServiceLog;

drop table Account;
drop table Item;
drop table ServiceLog;
drop table __EFMigrationsHistory;
*/