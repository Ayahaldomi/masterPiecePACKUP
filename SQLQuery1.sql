DROP DATABASE MasterPiece

USE master;
-- Create the database
CREATE DATABASE MasterPiece;
GO

-- Use the newly created database
USE MasterPiece;
GO




-- Create the Patients table
CREATE TABLE Patients (
    Patient_ID INT PRIMARY KEY identity(1,1),
    Full_Name NVARCHAR(MAX),
    Date_Of_Birth DATE,
    Gender NVARCHAR(MAX),
    Marital_Status NVARCHAR(MAX),
    Nationality NVARCHAR(MAX),
    Phone_Number INT,
    Home_Address NVARCHAR(MAX),
    Note NVARCHAR(MAX),
	PaymentStatus NVARCHAR(50) DEFAULT 'Unpaid'
);

-- Create the Tests table
CREATE TABLE Tests (
    Test_ID INT PRIMARY KEY identity(1,1),
    Test_Name NVARCHAR(MAX),
    Alternative_Name NVARCHAR(MAX) null,
	Components NVARCHAR(MAX),  -- Store components as a comma-separated string
    Normal_Range NVARCHAR(MAX),
	Unit nvarchar (max),
    Description NVARCHAR(MAX),
    Price DECIMAL(10, 2),
    Inventory DECIMAL(10, 2),
    Sample_Type nvarchar(max),
    Expiration_Date date
);

-- Create the Appointments table
CREATE TABLE Appointments (
    ID BIGINT PRIMARY KEY IDENTITY(1,1),
    Full_Name NVARCHAR(MAX),
    Gender NVARCHAR(MAX),
    Date_Of_Birth DATE,
    Email_Address NVARCHAR(MAX),
    Phone_Number NVARCHAR(MAX),
    Home_Address NVARCHAR(MAX),
    Date_Of_Appo DATE,
	Total_price decimal(10, 2),
	Amount_paid DECIMAL(10, 2),
    Billing_ID INT,
    Status NVARCHAR(MAX),
);
ALTER TABLE Appointments
ALTER COLUMN Date_Of_Appo DATETIME;

CREATE TABLE Appointments_Tests (
	ID INT PRIMARY KEY IDENTITY(1,1),
    Appointment_ID BIGINT,
    Test_ID INT,
    FOREIGN KEY (Appointment_ID) REFERENCES Appointments(ID)ON DELETE CASCADE,
    FOREIGN KEY (Test_ID) REFERENCES Tests(Test_ID)ON DELETE CASCADE,
);


-- Create the Packages table
CREATE TABLE Packages (
    Package_ID INT PRIMARY KEY IDENTITY(1,1),
    Package_Name NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10, 2),
    Picture NVARCHAR(MAX) -- Path or URL to the package image
);
ALTER TABLE Packages
ADD Old_price DECIMAL(10, 2); 


-- Create the Package_Tests table
CREATE TABLE Package_Tests (
    ID INT PRIMARY KEY IDENTITY(1,1),
    Package_ID INT,
    Test_ID INT,
    FOREIGN KEY (Package_ID) REFERENCES Packages(Package_ID) ON DELETE CASCADE,
    FOREIGN KEY (Test_ID) REFERENCES Tests(Test_ID) ON DELETE CASCADE
);

-- Create the Lab_Tech table
CREATE TABLE Lab_Tech (
    Tech_ID INT PRIMARY KEY identity(1,1),
    Name NVARCHAR(MAX),
    Email NVARCHAR(MAX),
    Password nvarchar(MAX),
    Status NVARCHAR(MAX)
);



-- Create the Test_Order table
CREATE TABLE Test_Order (
    Order_ID INT PRIMARY KEY identity(1,1),
    Patient_ID INT,
    Date DATE,
    Tech_ID INT,
	Total_Price DECIMAL(10, 2),
	Discount_Persent int DEFAULT 0,
	Amount_Paid DECIMAL(10, 2),
    Status NVARCHAR(MAX),
    FOREIGN KEY (Patient_ID) REFERENCES Patients(Patient_ID)ON DELETE CASCADE,
);

-- Create the Test_Order_Tests table
CREATE TABLE Test_Order_Tests (
    ID INT PRIMARY KEY identity(1,1),
    Order_ID INT,
    Test_ID INT,
    Result NVARCHAR(MAX),
    Date_Of_Result DATE,
    Comment NVARCHAR(MAX),
    Status NVARCHAR(MAX),
    FOREIGN KEY (Order_ID) REFERENCES Test_Order(Order_ID)ON DELETE CASCADE,
    FOREIGN KEY (Test_ID) REFERENCES Tests(Test_ID)ON DELETE CASCADE
);

CREATE TABLE Contact (
    contact_id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(max),
    email NVARCHAR(max),
    sub NVARCHAR(max),
    message NVARCHAR(max),
    sent_date DATE,
    admin_response NVARCHAR(max),
    response_date DATE,
    status INT
);

-- Create the Feedback table
CREATE TABLE Feedback (
    Feedback_ID INT PRIMARY KEY IDENTITY(1,1), -- Unique identifier for feedback
    Patient_ID INT, -- Foreign key referencing Patients table
    Message NVARCHAR(MAX), -- Feedback message from the patient
    Status NVARCHAR(50) DEFAULT 'Pending', -- Status of the feedback (e.g., Pending, Approved, Rejected)
    FOREIGN KEY (Patient_ID) REFERENCES Patients(Patient_ID) -- Establishing the foreign key relationship
);

CREATE TABLE ChatRooms (
    ChatRoom_ID INT PRIMARY KEY IDENTITY(1,1),
    LabTech_ID INT FOREIGN KEY REFERENCES Lab_Tech(Tech_ID),
    Patient_ID INT FOREIGN KEY REFERENCES Patients(Patient_ID),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE ChatMessages (
    ChatMessage_ID INT PRIMARY KEY IDENTITY(1,1),
    ChatRoom_ID INT FOREIGN KEY REFERENCES ChatRooms(ChatRoom_ID), -- New column linking to ChatRoom
    SenderId INT, -- Can be Patient_ID or Tech_ID
    MessageText NVARCHAR(MAX),
    SentAt DATETIME DEFAULT GETDATE(),
    SenderType NVARCHAR(MAX) -- 'Patient' or 'LabTech'
);
ALTER TABLE ChatRooms
ADD hasUnreadMessages BIT DEFAULT 0; -- 0 = No unread messages, 1 = Unread messages exist



INSERT INTO Patients (Full_Name, Date_Of_Birth, Gender, Marital_Status, Nationality, Phone_Number, Home_Address, Note)
VALUES ('John Doe', '1985-06-15', 'Male', 'Married', 'American', 1234567890, '123 Main St, New York', 'No allergies');

INSERT INTO Patients (Full_Name, Date_Of_Birth, Gender, Marital_Status, Nationality, Phone_Number, Home_Address, Note)
VALUES ('Jane Smith', '1990-11-30', 'Female', 'Single', 'British', 98765430, '456 Elm St, London', 'Allergic to penicillin');

INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Complete Blood Count', 'CBC', 'WBC,RBC,Platelets,Hemoglobin,Hematocrit,MCV,MCH,MCHC', '4.5-11.0,4.7-6.1,150-400,13.8-17.2,40.7-50.3,80-100,27-31,32-36', 'x10^9/L,x10^12/L,x10^9/L,g/dL,%,fL,pg,g/dL', 'A test used to evaluate overall health.', 50.00, 100, 'Blood', '2025-12-31');

INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Liver Function Test', 'LFT', 'ALT,AST,ALP,Bilirubin', '10-40,8-38,45-115,0.1-1.2', 'U/L,U/L,U/L,mg/dL', 'Tests to assess liver function.', 80.00, 80, 'Blood', '2026-06-30');

INSERT INTO Appointments ( Full_Name, Gender, Date_Of_Birth, Email_Address, Phone_Number, Home_Address, Date_Of_Appo, Total_price, Amount_paid, Billing_ID, Status)
VALUES ( 'John Doe', 'Male', '1985-06-15', 'john.doe@example.com', 1234567890, '123 Main St, New York', '2024-01-15', 130.00, 130.00, 1001, 'Completed');

INSERT INTO Appointments ( Full_Name, Gender, Date_Of_Birth, Email_Address, Phone_Number, Home_Address, Date_Of_Appo, Total_price, Amount_paid, Billing_ID, Status)
VALUES ( 'Jane Smith', 'Female', '1990-11-30', 'jane.smith@example.com', 9876543210, '456 Elm St, London', '2024-01-20', 200.00, 100.00, 1002, 'Pending');

INSERT INTO Appointments_Tests (Appointment_ID, Test_ID)
VALUES (1, 1);  -- John Doe's appointment with CBC test

INSERT INTO Appointments_Tests (Appointment_ID, Test_ID)
VALUES (2, 2);  -- Jane Smith's appointment with Liver Function Test (LFT)

INSERT INTO Lab_Tech (Name, Email, Password, Status)
VALUES ('Dr. Alice Brown', 'alice.brown@lab.com', 'password123', 'Active');

INSERT INTO Lab_Tech (Name, Email, Password, Status)
VALUES ('Dr. Bob Green', 'bob.green@lab.com', 'securepass456', 'Active');

INSERT INTO Test_Order (Patient_ID, Date, Tech_ID, Total_Price, Discount_Persent, Amount_Paid, Status)
VALUES (1, '2024-01-15', 1, 130.00, 10, 117.00, 'Completed');  -- 10% discount applied

INSERT INTO Test_Order (Patient_ID, Date, Tech_ID, Total_Price, Discount_Persent, Amount_Paid, Status)
VALUES (2, '2024-01-20', 2, 200.00, 0, 100.00, 'Pending');

INSERT INTO Test_Order_Tests (Order_ID, Test_ID, Result, Date_Of_Result, Comment, Status)
VALUES (1, 1, 'Normal', '2024-01-16', 'All values within normal range.', 'Completed');

INSERT INTO Test_Order_Tests (Order_ID, Test_ID, Result, Date_Of_Result, Comment, Status)
VALUES (2, 2, 'Elevated AST and ALT', '2024-01-21', 'Requires further investigation.', 'Pending');

-- Insert first package
INSERT INTO Packages (Package_Name, Description, Price, Picture)
VALUES ('Basic Health Checkup', 'This package includes basic health screening tests.', 99.99, '/images/basic_health_checkup.png');

-- Insert second package
INSERT INTO Packages (Package_Name, Description, Price, Picture)
VALUES ('Comprehensive Health Screening', 'This package offers a complete set of tests for in-depth health evaluation.', 199.99, '/images/comprehensive_health_screening.png');


-- Insert 1st record into Contact table
INSERT INTO Contact (name, email, sub, message, sent_date, admin_response, response_date, status)
VALUES ('John Doe', 'john.doe@example.com', 'Product Inquiry', 'I would like to know more about your product.', '2024-09-01', NULL, NULL, 0);

-- Insert 2nd record into Contact table
INSERT INTO Contact (name, email, sub, message, sent_date, admin_response, response_date, status)
VALUES ('Jane Smith', 'jane.smith@example.com', 'Support Request', 'I am facing an issue with my account.', '2024-09-05', 'We are looking into it.', '2024-09-06', 1);

-- Insert 1st record into Feedback table
INSERT INTO Feedback (Patient_ID, Message, Status)
VALUES (1, 'Great service! Really satisfied with the care provided.', 'Approved');

-- Insert 2nd record into Feedback table
INSERT INTO Feedback (Patient_ID, Message, Status)
VALUES (2, 'The appointment scheduling process needs improvement.', 'Pending');


-- Insert a chat room between the first lab tech and the first patient
INSERT INTO ChatRooms (LabTech_ID, Patient_ID, CreatedAt)
VALUES (1, 1, GETDATE()); -- Assuming the first lab tech (ID = 1) and first patient (ID = 1)

-- Insert another chat room between the second lab tech and the second patient
INSERT INTO ChatRooms (LabTech_ID, Patient_ID, CreatedAt)
VALUES (2, 2, GETDATE()); -- Assuming the second lab tech (ID = 2) and second patient (ID = 2)

-- Insert messages for the first chat room
INSERT INTO ChatMessages (ChatRoom_ID, SenderId, MessageText, SentAt, SenderType)
VALUES (1, 1, 'Hello Dr. Brown, I have a question about my lab results.', GETDATE(), 'Patient');

INSERT INTO ChatMessages (ChatRoom_ID, SenderId, MessageText, SentAt, SenderType)
VALUES (1, 1, 'Could you please explain the findings?', GETDATE(), 'Patient');

-- Insert messages for the second chat room
INSERT INTO ChatMessages (ChatRoom_ID, SenderId, MessageText, SentAt, SenderType)
VALUES (2, 2, 'Hello Dr. White, I would like to schedule a follow-up appointment.', GETDATE(), 'Patient');

INSERT INTO ChatMessages (ChatRoom_ID, SenderId, MessageText, SentAt, SenderType)
VALUES (2, 2, 'Can you check my recent lab results?', GETDATE(), 'Patient');




-- Erythrocyte Sedimentation Rate (ESR)
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Erythrocyte Sedimentation Rate', 'ESR', 'Sedimentation Rate', '0-20 mm/h', 'mm/h', 'A test that measures how quickly erythrocytes (red blood cells) settle at the bottom of a test tube.', 10.00, 100, 'Whole Blood', '2024-12-31');

-- Random Blood Glucose, Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Random Blood Glucose, Serum', NULL, 'Glucose', '70-140 mg/dL', 'mg/dL', 'A test that measures glucose levels in the blood at any time of the day.', 12.00, 100, 'Serum', '2024-12-31');

-- Calcium, Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Calcium, Serum', NULL, 'Calcium', '8.5-10.5 mg/dL', 'mg/dL', 'A test used to measure the amount of calcium in the blood.', 8.00, 100, 'Serum', '2024-12-31');

-- Ferritin, Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Ferritin, Serum', NULL, 'Ferritin', '20-500 ng/mL', 'ng/mL', 'A test that measures the amount of ferritin, which helps store iron in the body.', 20.00, 100, 'Serum', '2024-12-31');

-- Vitamin B12 (Cyanocobalamin), Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Vitamin B12 (Cyanocobalamin), Serum', 'Vitamin B12', 'Vitamin B12', '200-900 pg/mL', 'pg/mL', 'A test that measures the amount of Vitamin B12 in the blood.', 25.00, 100, 'Serum', '2024-12-31');

-- Urine Analysis
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES ('Urine Analysis', 'UA', 'pH, Glucose, Protein, Ketones, Bilirubin, Urobilinogen, Nitrites, Leukocytes', 'Varies by component', 'Varies', 'A test that checks the appearance, concentration, and content of urine for abnormalities.', 10.00, 100, 'Urine', '2024-12-31');






-- Inserting Magnesium, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Magnesium, Serum',
    NULL,
    'Magnesium',
    '1.7-2.2',
    'mg/dL',
    'This test evaluates the magnesium levels in your blood, which are important for muscle and nerve function, as well as bone strength.',
    18.00,
    40,
    'Serum',
    '2025-12-31'
);



-- Inserting Testosterone, Total, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Testosterone, Total, Serum',
    NULL,
    'Testosterone',
    '300-1000',
    'ng/dL',
    'This test measures the total testosterone level in your blood, which plays a key role in muscle mass, bone density, and sex drive in both men and women.',
    30.00,
    35,
    'Serum',
    '2025-10-01'
);

-- Inserting Zinc, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Zinc, Serum',
    NULL,
    'Zinc',
    '70-120',
    'mcg/dL',
    'This test measures the zinc level in your blood, essential for immune function, wound healing, and DNA synthesis.',
    25.00,
    45,
    'Serum',
    '2026-05-12'
);

-- Inserting Thyroid Stimulating Hormone (TSH), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Thyroid Stimulating Hormone (TSH), Serum',
    'TSH',
    'TSH',
    '0.4-4.0',
    'uIU/mL',
    'The TSH test measures the amount of thyroid-stimulating hormone in your blood, which helps regulate the production of thyroid hormones for metabolism.',
    28.00,
    70,
    'Serum',
    '2025-11-20'
);


-- Inserting Fasting Blood Sugar (Glucose), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Fasting Blood Sugar (Glucose), Serum',
    NULL,
    'Glucose',
    '70-100',
    'mg/dL',
    'This test measures the glucose level in your blood after an overnight fast. It helps diagnose and monitor diabetes and prediabetes.',
    15.00,
    80,
    'Serum',
    '2026-01-10'
);

-- Inserting C-Reactive Protein, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'C-Reactive Protein, Serum',
    'CRP',
    'C-Reactive Protein',
    '0-10',
    'mg/L',
    'CRP is a protein made by your liver and sent into your bloodstream in response to inflammation. This test helps detect inflammation in the body.',
    20.00,
    60,
    'Serum',
    '2025-12-01'
);

-- Inserting Urea, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Urea, Serum',
    NULL,
    'Urea',
    '7-20',
    'mg/dL',
    'This test measures the amount of urea nitrogen in your blood, which helps evaluate kidney function.',
    12.00,
    70,
    'Serum',
    '2025-12-31'
);

-- Inserting Creatinine, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Creatinine, Serum',
    NULL,
    'Creatinine',
    '0.7-1.3',
    'mg/dL',
    'The creatinine test is used to assess kidney function by measuring the level of creatinine in your blood.',
    15.00,
    75,
    'Serum',
    '2026-02-15'
);

-- Inserting Estimated Glomerular Filtration Rate (eGFR), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Estimated Glomerular Filtration Rate (eGFR), Serum',
    'eGFR',
    'Glomerular Filtration Rate',
    '>90',
    'mL/min/1.73 m²',
    'This test estimates how well your kidneys are filtering waste from your blood. It is calculated using your creatinine results.',
    18.00,
    65,
    'Serum',
    '2025-11-30'
);

-- Inserting Uric Acid, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Uric Acid, Serum',
    NULL,
    'Uric Acid',
    '3.5-7.2',
    'mg/dL',
    'This test measures the amount of uric acid in your blood, which can help detect conditions like gout and kidney disease.',
    16.00,
    50,
    'Serum',
    '2026-03-01'
);

-- Inserting Sodium, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Sodium, Serum',
    NULL,
    'Sodium',
    '135-145',
    'mEq/L',
    'This test measures the sodium levels in your blood, which are crucial for proper muscle and nerve function.',
    10.00,
    90,
    'Serum',
    '2026-01-01'
);

-- Inserting Potassium, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Potassium, Serum',
    NULL,
    'Potassium',
    '3.5-5.1',
    'mEq/L',
    'This test measures the amount of potassium in your blood, which is important for heart function and muscle contraction.',
    10.00,
    90,
    'Serum',
    '2026-01-01'
);

-- Inserting Chloride, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Chloride, Serum',
    NULL,
    'Chloride',
    '98-107',
    'mEq/L',
    'This test measures the level of chloride in your blood, important for maintaining proper fluid balance and pH.',
    10.00,
    85,
    'Serum',
    '2026-02-20'
);

-- Inserting Cholesterol, Total, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Cholesterol, Total, Serum',
    NULL,
    'Cholesterol',
    '<200',
    'mg/dL',
    'This test measures the total amount of cholesterol in your blood, a key indicator of heart disease risk.',
    25.00,
    100,
    'Serum',
    '2026-01-10'
);

-- Inserting Triglycerides, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Triglycerides, Serum',
    NULL,
    'Triglycerides',
    '<150',
    'mg/dL',
    'This test measures the amount of triglycerides in your blood, which are a type of fat associated with heart disease risk.',
    22.00,
    95,
    'Serum',
    '2025-12-20'
);

-- Inserting Alanine Aminotransferase (ALT/GPT), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Alanine Aminotransferase (ALT/GPT), Serum',
    'ALT/GPT',
    'Alanine Aminotransferase',
    '7-55',
    'U/L',
    'The ALT test measures liver enzyme levels, which can indicate liver damage or disease.',
    18.00,
    85,
    'Serum',
    '2026-03-10'
);

-- Inserting Aspartate Aminotransferase (AST/GOT), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Aspartate Aminotransferase (AST/GOT), Serum',
    'AST/GOT',
    'Aspartate Aminotransferase',
    '10-40',
    'U/L',
    'The AST test measures the enzyme level in your blood, which is primarily found in the liver and muscles, helping to diagnose liver conditions.',
    18.00,
    85,
    'Serum',
    '2026-03-10'
);

-- Inserting Gamma-Glutamyl Transferase (g-GT), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Gamma-Glutamyl Transferase (g-GT), Serum',
    'g-GT',
    'Gamma-Glutamyl Transferase',
    '9-48',
    'U/L',
    'This test measures the level of g-GT, an enzyme in your blood, which helps detect liver disease or bile duct problems.',
    20.00,
    75,
    'Serum',
    '2026-02-01'
);

-- Inserting Alkaline Phosphatase, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Alkaline Phosphatase, Serum',
    NULL,
    'Alkaline Phosphatase',
    '44-147',
    'U/L',
    'The alkaline phosphatase test helps diagnose liver or bone disorders by measuring the level of this enzyme in your blood.',
    17.00,
    90,
    'Serum',
    '2026-01-30'
);

-- Inserting Bilirubin, Total, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Bilirubin, Total, Serum',
    NULL,
    'Bilirubin',
    '0.1-1.2',
    'mg/dL',
    'This test measures the total amount of bilirubin in your blood, used to diagnose liver disease or hemolytic anemia.',
    15.00,
    65,
    'Serum',
    '2025-11-15'
);

-- Inserting Bilirubin, Direct, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Bilirubin, Direct, Serum',
    NULL,
    'Direct Bilirubin',
    '0-0.3',
    'mg/dL',
    'The direct bilirubin test measures the conjugated bilirubin level in your blood, useful for diagnosing liver and bile duct diseases.',
    15.00,
    65,
    'Serum',
    '2025-11-15'
);

-- Inserting Protein, Total, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Protein, Total, Serum',
    NULL,
    'Total Protein',
    '6.4-8.3',
    'g/dL',
    'This test measures the total amount of protein in your blood, including albumin and globulin. It helps diagnose liver and kidney disease.',
    18.00,
    70,
    'Serum',
    '2026-04-01'
);

-- Inserting High Density Lipoprotein (HDL) Cholesterol, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'High Density Lipoprotein (HDL) Cholesterol, Serum',
    'HDL Cholesterol',
    'HDL Cholesterol',
    '>60',
    'mg/dL',
    'The HDL cholesterol test measures the "good" cholesterol level in your blood, helping assess heart disease risk.',
    20.00,
    90,
    'Serum',
    '2026-02-20'
);

-- Inserting Cholesterol / HDL Ratio, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Cholesterol / HDL Ratio, Serum',
    NULL,
    'Cholesterol/HDL Ratio',
    '<4.5',
    'Ratio',
    'This test compares the total cholesterol to HDL cholesterol ratio, which helps estimate heart disease risk.',
    20.00,
    85,
    'Serum',
    '2026-03-01'
);

-- Inserting Non HDL Cholesterol, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Non HDL Cholesterol, Serum',
    NULL,
    'Non-HDL Cholesterol',
    '<130',
    'mg/dL',
    'This test measures the cholesterol in your blood that is not HDL cholesterol, which is associated with higher heart disease risk.',
    20.00,
    90,
    'Serum',
    '2026-02-25'
);

-- Inserting Low Density Lipoprotein (LDL) Cholesterol, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Low Density Lipoprotein (LDL) Cholesterol, Serum',
    'LDL Cholesterol',
    'LDL Cholesterol',
    '<100',
    'mg/dL',
    'The LDL cholesterol test measures the "bad" cholesterol in your blood, a major risk factor for heart disease.',
    25.00,
    80,
    'Serum',
    '2026-01-15'
);


-- Inserting Complete Blood Count (CBC) (without blood film) Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Complete Blood Count (CBC) (without blood film)',
    'CBC',
    'WBC, RBC, Hemoglobin, Hematocrit, Platelet Count',
    'Varies by component',
    'cells/mcL',
    'The CBC test evaluates the different types of cells in your blood and helps detect conditions such as anemia, infection, and many other disorders.',
    30.00,
    150,
    'Whole Blood',
    '2026-01-15'
);

-- Inserting Insulin, Fasting, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Insulin, Fasting, Serum',
    NULL,
    'Insulin',
    '2.6-24.9',
    'uIU/mL',
    'This test measures the insulin level in your blood after fasting and helps evaluate insulin production and resistance.',
    25.00,
    80,
    'Serum',
    '2026-02-10'
);

-- Inserting Homa Score (HOMA2-IR), Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Homa Score (HOMA2-IR), Serum',
    'HOMA-IR',
    'HOMA2-IR Score',
    '<1.0 (for normal insulin sensitivity)',
    'Score',
    'The HOMA-IR test estimates insulin resistance, helping to assess conditions like diabetes and metabolic syndrome.',
    35.00,
    50,
    'Serum',
    '2026-03-01'
);

-- Inserting 25-hydroxy Vitamin D Total, Serum Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    '25-hydroxy Vitamin D Total, Serum',
    'Vitamin D',
    '25-hydroxy Vitamin D',
    '30-100',
    'ng/mL',
    'This test measures the total amount of vitamin D in your blood to assess bone health, immune function, and overall health.',
    40.00,
    60,
    'Serum',
    '2026-04-05'
);


-- Inserting Cardiovascular Disease Associated Risk, 12 Mutation Profile, PCR Test
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Cardiovascular Disease Associated Risk, 12 Mutation Profile, PCR',
    'CVD Risk Mutation Profile',
    '12 Mutation Profile',
    'N/A',
    'Genetic Variants',
    'This test analyzes 12 genetic mutations associated with cardiovascular disease risk using PCR technology. It helps in identifying individuals at risk for cardiovascular conditions based on genetic predispositions.',
    150.00,
    30,
    'Whole Blood or Buccal Swab',
    '2026-05-01'
);


-- Blood Group and Rh Typing
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Blood Group and Rh Typing',
    'Blood Type and Rh Factor',
    'ABO Group, Rh Factor',
    'ABO: A/B/O/AB, Rh: Positive/Negative',
    'N/A',
    'This test identifies the blood group (A, B, O, or AB) and Rh factor (positive or negative) which is crucial for transfusions, pregnancies, and medical procedures.',
    25.00,
    50,
    'Whole Blood',
    '2025-12-01'
);



-- Rubella Antibodies, IgG, Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Rubella Antibodies, IgG, Serum',
    'Rubella IgG Antibodies',
    'Rubella IgG Antibodies',
    'Positive: >10 IU/mL',
    'IU/mL',
    'This test detects IgG antibodies to Rubella, indicating previous infection or vaccination, crucial for assessing immunity, particularly in pregnant women.',
    30.00,
    80,
    'Serum',
    '2025-10-15'
);




-- Hepatitis B Surface Antigen (HBsAg), Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Hepatitis B Surface Antigen (HBsAg), Serum',
    'HBsAg Test',
    'Hepatitis B Surface Antigen',
    'Negative',
    'N/A',
    'This test detects the presence of Hepatitis B Surface Antigen in the blood, used to screen for and diagnose Hepatitis B infection.',
    40.00,
    100,
    'Serum',
    '2026-01-01'
);

-- Hepatitis C Virus Total Antibodies, Serum
INSERT INTO Tests (Test_Name, Alternative_Name, Components, Normal_Range, Unit, Description, Price, Inventory, Sample_Type, Expiration_Date)
VALUES (
    'Hepatitis C Virus Total Antibodies, Serum',
    'HCV Antibodies Test',
    'HCV Antibodies',
    'Negative',
    'N/A',
    'This test detects antibodies against Hepatitis C virus (HCV) in the blood, indicating past or current infection.',
    45.00,
    90,
    'Serum',
    '2025-12-15'
);
