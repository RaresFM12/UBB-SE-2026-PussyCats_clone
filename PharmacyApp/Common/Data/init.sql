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

-- some inserts
INSERT INTO Substances(name, lethalDose, description)
VALUES
	('Ibuprofen', 3200.00, 'Anti-inflammatory pain reliever'),
	('Paracetamol', 4000.00, 'Pain reliever and fever reducer'),
	('Magnesium', 2500.00, 'Mineral supplement for muscle and nerve support'),
	('Iron', 45.00, 'Mineral supplement used for iron deficiency'),
	('Vitamin C', 2000.00, 'Vitamin supplement for immune support'),
	('Calcium', 2500.00, 'Mineral supplement for bones and muscles'),
	('Omega 3', 3000.00, 'Fatty acid supplement for heart and brain health'),
	('Melatonin', 10.00, 'Sleep support supplement'),
	('Probiotics', 1000.00, 'Digestive support supplement'),
	('Zinc', 40.00, 'Mineral supplement for immunity');

INSERT INTO Items
(name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage)
VALUES
	('Nurofen Express', 28.50, 'pain relief', 20, 'Reckitt', 'Assets/nurofen.png', 40, 'Fast pain relief', 'Ibuprofen capsules for pain and inflammation', 0),
	('Panadol Extra', 19.99, 'pain relief', 16, 'GSK', 'Assets/panadol.png', 35, 'Extra strength', 'Paracetamol tablets for headaches and fever', 10),
	('Magne B6', 32.00, 'wellness', 50, 'Sanofi', 'Assets/magneb6.png', 25, 'Magnesium support', 'Magnesium and vitamin B6 supplement', 0),
	('Feroglobin', 36.50, 'wellness', 30, 'Vitabiotics', 'Assets/feroglobin.png', 18, 'Iron formula', 'Iron supplement for energy and blood health', 5),
	('Vitamin C 1000', 22.00, 'wellness', 20, 'NaturPharma', 'Assets/vitaminc.png', 50, 'Immune support', 'High strength vitamin C tablets', 0),
	('Calcium + D3', 27.50, 'wellness', 30, 'BioFarm', 'Assets/calciumd3.png', 22, 'Bone support', 'Calcium and vitamin D3 supplement', 15),
	('Omega 3 Forte', 45.00, 'wellness', 60, 'Doppelherz', 'Assets/omega3.png', 14, 'Heart support', 'Omega 3 capsules for heart and brain', 0),
	('Melatonin Sleep', 18.00, 'wellness', 30, 'Walmark', 'Assets/melatonin.png', 12, 'Sleep support', 'Melatonin tablets for better sleep', 0),
	('Probiotic Balance', 39.99, 'wellness', 20, 'Secom', 'Assets/probiotic.png', 16, 'Digestive comfort', 'Daily probiotic capsules', 20),
	('Zinc Complex', 21.50, 'wellness', 30, 'NaturMil', 'Assets/zinc.png', 28, 'Immune defense', 'Zinc supplement for immune support', 0),
	('Coldrex MaxGrip', 31.00, 'cold and flu', 10, 'GSK', 'Assets/coldrex.png', 20, 'Cold relief', 'Powder for cold and flu symptoms', 0),
	('Strepsils Intensive', 24.00, 'cold and flu', 24, 'Reckitt', 'Assets/strepsils.png', 17, 'Sore throat relief', 'Lozenges for sore throat', 0),
	('No-Spa Forte', 26.00, 'pain relief', 24, 'Sanofi', 'Assets/nospa.png', 30, 'Cramp relief', 'Drotaverine tablets for cramps', 0),
	('Femina Comfort', 29.50, 'wellness', 30, 'HerbalLab', 'Assets/femina.png', 19, 'Period wellness', 'Supplement designed for menstrual comfort', 10),
	('Herbal Relax Tea Capsules', 23.50, 'wellness', 20, 'PlantMed', 'Assets/herbalrelax.png', 21, 'Relax support', 'Natural calming capsules for stress relief', 0);

INSERT INTO ItemSubstances(itemId, name, concentration)
VALUES
	(1, 'Ibuprofen', 400.00),
	(2, 'Paracetamol', 500.00),
	(3, 'Magnesium', 250.00),
	(4, 'Iron', 14.00),
	(5, 'Vitamin C', 1000.00),
	(6, 'Calcium', 500.00),
	(7, 'Omega 3', 1000.00),
	(8, 'Melatonin', 5.00),
	(9, 'Probiotics', 200.00),
	(10, 'Zinc', 10.00),
	(11, 'Paracetamol', 1000.00),
	(12, 'Ibuprofen', 8.75),
	(13, 'Ibuprofen', 80.00),
	(14, 'Magnesium', 150.00),
	(14, 'Vitamin C', 80.00),
	(15, 'Magnesium', 100.00);


INSERT INTO ItemExpirationDates(itemId, expirationDate, numberOfPacks)
VALUES
	(1, '2026-08-15', 20),
	(1, '2027-01-10', 20),

	(2, '2026-09-20', 15),
	(2, '2027-02-15', 20),

	(3, '2026-10-05', 10),
	(3, '2027-03-01', 15),

	(4, '2026-11-12', 8),
	(4, '2027-04-18', 10),

	(5, '2026-07-30', 25),
	(5, '2027-01-25', 25),

	(6, '2026-12-10', 10),
	(6, '2027-05-05', 12),

	(7, '2026-09-01', 6),
	(7, '2027-06-14', 8),

	(8, '2026-08-22', 5),
	(8, '2027-02-28', 7),

	(9, '2026-10-18', 8),
	(9, '2027-03-20', 8),

	(10, '2026-11-30', 12),
	(10, '2027-04-30', 16),

	(11, '2026-09-09', 10),
	(11, '2027-01-19', 10),

	(12, '2026-10-25', 7),
	(12, '2027-05-10', 10),

	(13, '2026-08-08', 15),
	(13, '2027-02-02', 15),

	(14, '2026-12-22', 9),
	(14, '2027-06-01', 10),

	(15, '2026-09-17', 10),
	(15, '2027-03-11', 11);

SELECT * FROM Items;
SELECT * FROM Items WHERE category = 'wellness';
SELECT * FROM ItemSubstances;
SELECT * FROM ItemExpirationDates;
SELECT * FROM PeriodTrackers;





