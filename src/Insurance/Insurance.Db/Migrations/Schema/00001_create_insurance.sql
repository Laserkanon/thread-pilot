CREATE TABLE dbo.Insurance (
  InsuranceId BIGINT NOT NULL IDENTITY(1000000,1) PRIMARY KEY,
  PersonalIdentityNumber CHAR(12) NOT NULL,
  Product SMALLINT NOT NULL,
  MonthlyCost MONEY NOT NULL,
  CarRegistrationNumber VARCHAR(16) NULL
);

CREATE NONCLUSTERED INDEX IX_Insurance_Pin ON dbo.Insurance (PersonalIdentityNumber);
