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

