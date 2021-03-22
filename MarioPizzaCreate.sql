create table bottom
(
	id int identity
		primary key,
	name varchar(45) not null,
	diameter int not null
)
go

create table countrycode
(
	id int identity
		primary key,
	code varchar(2) default 'NL'
)
go

create table bottomprice
(
	id int identity
		primary key,
	bottomid int not null
		constraint fk_bottomprice_bottom
			references bottom,
	price decimal(4,2) not null,
	currency varchar(3) not null,
	startdate datetime not null,
	vat decimal(4,2) not null,
	countrycodeid int not null
		constraint fk_bottomprice_countrycode1
			references countrycode
)
go

create index fk_bottomprice_countrycode1_idx
	on bottomprice (countrycodeid)
go

create index fk_bottomprice_bottom_idx
	on bottomprice (bottomid)
go

create table deliverytype
(
	id int identity
		primary key,
	name varchar(45) not null
)
go

create table import_log
(
	id int identity
		constraint import_log_pk
			primary key nonclustered,
	time datetime default getdate(),
	filename varchar(100) not null,
	error text,
	type varchar(10) not null
)
go

create table ingredient
(
	id int identity
		primary key,
	name varchar(45) not null,
	isvegetarian bit default '0' not null,
	isspicy bit default '0' not null
)
go

create table ingredientprice
(
	id int not null
		primary key identity(1,1),
	ingredientid int not null
		constraint fk_ingredientprice_ingredient
			references ingredient,
	price decimal(4,2) not null,
	startdate datetime not null,
	currency varchar(3) not null,
	vat decimal(4,2) not null,
	countrycodeid int not null
		constraint fk_ingredientprice_countrycode1
			references countrycode
)
go

create index fk_ingredientprice_countrycode1_idx
	on ingredientprice (countrycodeid)
go

create index fk_ingredientprice_ingredient_idx
	on ingredientprice (ingredientid)
go

create table postalcode_import
(
	postalcode varchar(20) not null,
	iseven decimal not null,
	startingnumber decimal not null,
	endingnumber decimal not null,
	city varchar(255) not null,
	street varchar(255),
	township varchar(255)
)
go

create table productcategory
(
	id int identity
		primary key,
	name varchar(45) not null,
	parentproductcategoryid int default NULL
		constraint fk_productcategory_productcategory
			references productcategory
)
go

create table coupon
(
	id int not null
		primary key,
	code varchar(20) not null,
	startdate datetime not null,
	enddate datetime not null,
	amount decimal(5,2) default NULL,
	percentage decimal(5,2) default NULL,
	applyon int default NULL,
	description varchar(100),
	productcategoryid int default NULL
		constraint fk_coupon_productcategory
			references productcategory,
	isfordeliverytypeonly int default NULL
		constraint fk_coupon_deliverytype
			references deliverytype,
	pricetreshold decimal(5,2) default NULL
)
go

create index fk_coupon_deliverytype_idx
	on coupon (isfordeliverytypeonly)
go

create index fk_coupon_productcategory_idx
	on coupon (productcategoryid)
go

create index fk_productcategory_productcategory_idx
	on productcategory (parentproductcategoryid)
go

create table sauce
(
	id int identity
		primary key,
	name varchar(45)
)
go

create table product
(
	id int identity
		primary key,
	name varchar(45) not null,
	sauceid int
		constraint fk_pizza_sauce
			references sauce,
	productcategory int not null
		constraint fk_product_productsubcategory
			references productcategory,
	isvegetarian bit default '0' not null,
	isspicy bit default '0' not null
)
go

create index fk_product_productcategory_idx
	on product (productcategory)
go

create index fk_pizza_sauce_idx
	on product (sauceid)
go

create table productingredient
(
	productid int not null
		constraint fk_productingredient_product
			references product,
	ingredientid int not null
		constraint fk_productingredient_ingredient
			references ingredient,
	amount int default 1 not null,
	primary key (productid, ingredientid)
)
go

create index fk_productingredient_ingredient_idx
	on productingredient (ingredientid)
go

create index fk_productingredient_product_idx
	on productingredient (productid)
go

create table productprice
(
	id int identity
		primary key,
	productid int not null
		constraint fk_productprice_product
			references product,
	price decimal(4,2) not null,
	currency varchar(3) default 'EUR' not null,
	startdate datetime not null,
	vat decimal(4,2) not null,
	countrycodeid int not null
		constraint fk_productprice_countrycode
			references countrycode
)
go

create index fk_productprice_countrycode_idx
	on productprice (countrycodeid)
go

create index fk_pizzaprice_pizza_idx
	on productprice (productid)
go

create table township
(
	id int identity
		primary key,
	name varchar(45)
)
go

create table postalcode
(
	id int identity
		primary key,
	postalcode varchar(20) not null,
	iseven bit not null,
	startingnumber int not null,
	endingnumber int not null,
	city varchar(255) not null,
	street varchar(255),
	townshipid int not null
		constraint fk_postalcode_township
			references township
)
go

create table address
(
	id int identity
		primary key,
	number varchar(10) not null,
	countrycode varchar(2) default 'NL',
	postalcodeid int
		constraint fk_address_postalcode
			references postalcode,
	street varchar(255) not null
)
go

create index fk_address_postalcode_idx
	on address (postalcodeid)
go

create index fk_postalcode_township_idx
	on postalcode (townshipid)
go

create index postalcode_postalcode_startingnumber_endingnumber_index
	on postalcode (postalcode, startingnumber, endingnumber)
go

create index index_street_match
	on postalcode (street, startingnumber, endingnumber)
go

create index postalcode_city_index
	on postalcode (city)
go

create table store
(
	id int identity
		primary key,
	name varchar(45),
	addressid int not null
		constraint fk_store_address
			references address,
	phonenumber varchar(45) not null,
	townshipid int not null
		constraint fk_store_township
			references township
)
go

create index fk_store_address_idx
	on store (addressid)
go

create index township_name_index
	on township (name)
go

create table [user]
(
	id int identity
		primary key,
	name varchar(45) not null,
	email varchar(45) not null,
	addressid int not null
		constraint fk_user_address
			references address,
	password varchar(255) not null
)
go

create table [order]
(
	id int not null
		primary key,
	datecreated datetime default getdate() not null,
	datedelivered datetime default NULL,
	phonenumber varchar(45) default NULL,
	clientname varchar(100) default NULL,
	userid int default NULL
		constraint fk_order_client
			references [user],
	couponid int default NULL
		constraint fk_order_coupon
			references coupon,
	remark varchar(255),
	deliverytypeid int not null
		constraint fk_order_deliverytype
			references deliverytype,
	addressid int not null
		constraint fk_order_address
			references address,
	storeid int not null
		constraint fk_order_store
			references store,
	preferredtime datetime not null,
	price decimal(5,2) default '0,0' not null,
	discount decimal(5,2) default '0,0' not null
)
go

create index fk_order_deliverytype_idx
	on [order] (deliverytypeid)
go

create index fk_order_store_idx
	on [order] (storeid)
go

create index fk_order_address_idx
	on [order] (addressid)
go

create index fk_order_coupon_idx
	on [order] (couponid)
go

create index fk_order_client_idx
	on [order] (userid)
go

create table orderline
(
	id int identity
		primary key,
	productid int not null
		constraint fk_productorder_pizza
			references product,
	orderid int not null
		constraint fk_productorder_order
			references [order],
	bottomid int
		constraint fk_productorder_bottom
			references bottom,
	amount int default 1 not null,
	price decimal(5,2) default '0,0' not null
)
go

create index fk_orderpizza_bottom_idx
	on orderline (bottomid)
go

create index fk_orderpizza_pizza_idx
	on orderline (productid)
go

create index fk_orderpizza_order_idx
	on orderline (orderid)
go

create table productorderingredient
(
	productorderid int not null
		constraint pizzaorderingredients_orderpizza
			references orderline,
	ingredientid int not null
		constraint pizzaorderingredients_ingredients
			references ingredient,
	amount int default 1 not null,
	primary key (productorderid, ingredientid)
)
go

create index pizzaorderingredients_ingredients_idx
	on productorderingredient (ingredientid)
go

create index pizzaorderingredients_orderpizza_idx
	on productorderingredient (productorderid)
go

create table productordersauce
(
	productorderid int not null
		constraint fk_productordersauce_productorder
			references orderline,
	sauceid int not null
		constraint fk_productordersauce_sauce
			references sauce,
	amount int default 1 not null,
	primary key (productorderid, sauceid)
)
go

CREATE TABLE mapping
(
    id           INT          NOT NULL IDENTITY PRIMARY KEY,
    originalname VARCHAR(255) NOT NULL,
    mappedto     VARCHAR(255) NULL,
    isingredient BIT          NOT NULL
)
go

create index fk_productordersauce_sauce_idx
	on productordersauce (sauceid)
go

create index fk_productordersauce_productorder_idx
	on productordersauce (productorderid)
go

create index fk_user_address_idx
	on [user] (addressid)
go

CREATE PROCEDURE ImportPostalCode
AS
    DECLARE CursorPostalCodeImport CURSOR LOCAL
    FOR (SELECT postalcode, iseven, startingnumber, endingnumber, city, street, township FROM postalcode_import);

    BEGIN
        OPEN CursorPostalCodeImport;

        DECLARE @postalcode VARCHAR(40),
            @isEven INTEGER,
            @startingNumber INTEGER,
            @endingNumber INTEGER,
            @city VARCHAR(255),
            @street VARCHAR(255),
            @township VARCHAR(255),
            @errorString VARCHAR(2000);

        SET NOCOUNT ON;

        FETCH NEXT FROM CursorPostalCodeImport INTO @postalcode, @isEven, @startingNumber, @endingNumber, @city, @street, @township;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Format strings for consistency
            SET @postalcode = REPLACE(@postalcode, ' ', '');

            DECLARE @townshipIdCurrent INT;
            SET @townshipIdCurrent = (SELECT TOP(1) t.id FROM township t WHERE t.name = @township);

            IF @townshipIdCurrent IS NULL
            BEGIN
                INSERT INTO township (name) VALUES (@township);
                SET @townshipIdCurrent = (SELECT @@IDENTITY);
            END

            IF NOT EXISTS (SELECT id FROM postalcode WHERE postalcode = @postalcode AND startingnumber = @startingNumber AND endingnumber = @endingNumber)
            BEGIN
                INSERT INTO postalcode (postalcode, iseven, startingnumber, endingnumber, city, street, townshipid)
                    VALUES (@postalcode, @isEven, @startingNumber, @endingNumber, @city, @street, @townshipIdCurrent);
            END
            ELSE
            BEGIN
                SET @errorString = CONCAT('Skipped ', @postalcode, ', ', @startingNumber, '-', @endingNumber, ' (isEven: ', @isEven, ') due to duplication.');

                EXEC SaveLog @type = 'ERROR', @fileName = 'postalcode_import', @errorString = @errorString;
            END;

            FETCH NEXT FROM CursorPostalCodeImport INTO @postalcode, @isEven, @startingNumber, @endingNumber, @city, @street, @township;
        END

        CLOSE CursorPostalCodeImport;
    END
go

CREATE PROCEDURE SaveLog (@type VARCHAR(10), @fileName VARCHAR(100), @errorString TEXT)
AS
BEGIN
    IF (@type = 'ERROR' OR @type = 'CORRECTION')
    BEGIN
        IF @errorString IS NULL
        BEGIN
            RAISERROR ('When inserting error logs, an errorString is required.', 16, 1);
        END;
    END;

    INSERT INTO import_log (time, filename, error, type) VALUES (CURRENT_TIMESTAMP, @fileName, @errorString, @type);
END
go

