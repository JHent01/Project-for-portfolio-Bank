create table TransactionHistory (
	IdAccount int not null ,
	Balance decimal(19,4) DEFAULT 0 CHECK (Balance>=0) ,
	FOREIGN KEY (IdAccount) REFERENCES AccountCard(IdAccount)


);