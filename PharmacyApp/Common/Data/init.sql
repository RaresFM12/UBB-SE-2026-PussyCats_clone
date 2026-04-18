create database Pharmacy
go
use Pharmacy
go

create table Substances(
	name varchar(255) primary key,
	lethalDose decimal(10,2),
	description varchar(200)
)

create table Items(
	--ids will be autoincremented
	itemId int identity(1,1) primary key,
	name varchar(255) not null,
	price decimal(10,2),
	category varchar(255),
	numberOfPills int,
	producer varchar(255),
	--image somehow
	imagePath varchar(255),
	quantity int,
	label varchar(255),
	description varchar(255),
	discountPercentage decimal(10,2)
)

create table ItemSubstances(
	itemId int references Items(itemId),
	name varchar(255) references Substances(name),
	concentration decimal(10,2),
	primary key (itemId,name)
)

create table ItemExpirationDates(
	itemId int references Items(itemId),
	expirationDate date,
	numberOfPacks int,
	primary key (itemId,expirationDate)
)

create table Users(
	userId int identity(1,1) primary key,
	email varchar(255) unique,
	phoneNumber varchar(255),
	passwordHash varchar(255),
	isDisabled bit not null,
	isAdmin bit not null,
	username varchar(255),
	discountNotifications bit not null,
	--loyalty points? do we have these? cannot find them in features
	loyaltyPoints int
)

create table UserDiscounts(
	userId int references Users(userId),
	itemId int references Items(itemId),
	itemDiscountPercentage decimal(10,2),
	primary key(userId,itemId)
)

create table UserNotifications(
	userId int references Users(userId),
	itemId int references Items(itemId),
	--favouriteItem? noi mai avem astea macar?
	favouriteItem bit not null,
	stockAlert bit not null,
	primary key(userId,itemId)
)

create table PeriodNotes(
	userId int references Users(userId),
	noteId int,
	noteBody varchar(255),
	isDone bit not null,
	primary key(userId,noteId)

)
create table PeriodTrackers(
	userId int references Users(userId) primary key,
	startPeriodDate date,
	cycleDays int,
	periodLasts int,
	PMSOption int
)

create table Orders(
	orderId int identity(1,1) primary key,
	clientId int references Users(userId),
	isCompleted bit not null,
	isExpired bit not null,
	pickUpDate date
)

create table OrderItems(
	orderId int references Orders(orderId),
	itemId int references Items(itemId),
	orderQuantity int,
	price decimal(10,2),
	primary key(orderId,itemId)
)





