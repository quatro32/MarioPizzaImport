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
            @township VARCHAR(255);

        SET NOCOUNT ON;

        FETCH NEXT FROM CursorPostalCodeImport INTO @postalcode, @isEven, @startingNumber, @endingNumber, @city, @street, @township;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @townshipIdCurrent INT;
            SET @townshipIdCurrent = (SELECT TOP(1) t.id FROM township t WHERE t.name = @township);

            BEGIN TRANSACTION
            BEGIN TRY
                IF @townshipIdCurrent IS NULL
                BEGIN
                    INSERT INTO township (name) VALUES (@township);
                    SET @townshipIdCurrent = (SELECT @@IDENTITY);
                END

                INSERT INTO postalcode (postalcode, iseven, startingnumber, endingnumber, city, street, townshipid)
                    VALUES (REPLACE(@postalcode, ' ', ''), @isEven, @startingNumber, @endingNumber, @city, @street, @townshipIdCurrent);
                COMMIT

                FETCH NEXT FROM CursorPostalCodeImport INTO @postalcode, @isEven, @startingNumber, @endingNumber, @city, @street, @township;
            END TRY
            BEGIN CATCH
                ROLLBACK TRANSACTION
                CLOSE CursorPostalCodeImport;
            END CATCH
        END

        CLOSE CursorPostalCodeImport;
        
        TRUNCATE TABLE postalcode_import;
    END
go

