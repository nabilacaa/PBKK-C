/* =====================================================================
   StudentManagementDB — Script database tugas PBKK (SQL Server)
   Jalankan SELURUH script sekaligus (SSMS: F5, VS Code mssql: Ctrl+Shift+E).
   Script aman dijalankan berulang kali karena objek lama dihapus lebih dulu.
   ===================================================================== */

/* ---------- 1. Database ---------- */
IF DB_ID('StudentManagementDB') IS NULL
    CREATE DATABASE StudentManagementDB;
GO

USE StudentManagementDB;
GO

/* ---------- 2. Hapus objek lama (urutan: view, tabel anak, tabel induk) ---------- */
IF OBJECT_ID('dbo.vw_EnrollmentDetail', 'V') IS NOT NULL DROP VIEW dbo.vw_EnrollmentDetail;
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.Students',    'U') IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID('dbo.Courses',     'U') IS NOT NULL DROP TABLE dbo.Courses;
IF OBJECT_ID('dbo.Programs',    'U') IS NOT NULL DROP TABLE dbo.Programs;
GO

/* ---------- 3. Tabel master: Programs (program studi) ---------- */
CREATE TABLE dbo.Programs
(
    ProgramId   INT IDENTITY(1,1)
        CONSTRAINT PK_Programs PRIMARY KEY,
    ProgramCode VARCHAR(10)  NOT NULL
        CONSTRAINT UQ_Programs_ProgramCode UNIQUE,
    ProgramName VARCHAR(100) NOT NULL,
    Faculty     VARCHAR(100) NOT NULL,
    IsActive    BIT          NOT NULL
        CONSTRAINT DF_Programs_IsActive DEFAULT (1),
    CreatedAt   DATETIME2    NOT NULL
        CONSTRAINT DF_Programs_CreatedAt DEFAULT SYSDATETIME()
);
GO

/* ---------- 4. Tabel master: Students (mahasiswa), relasi N:1 ke Programs ---------- */
CREATE TABLE dbo.Students
(
    StudentId   INT IDENTITY(1,1)
        CONSTRAINT PK_Students PRIMARY KEY,
    NRP         CHAR(10)      NOT NULL
        CONSTRAINT UQ_Students_NRP UNIQUE,
    FullName    NVARCHAR(150) NOT NULL,
    Gender      CHAR(1)       NOT NULL
        CONSTRAINT CK_Students_Gender CHECK (Gender IN ('L', 'P')),
    Email       VARCHAR(150)  NOT NULL
        CONSTRAINT UQ_Students_Email UNIQUE,
    EntryYear   SMALLINT      NOT NULL
        CONSTRAINT CK_Students_EntryYear CHECK (EntryYear BETWEEN 2000 AND 2100),
    PhoneNumber VARCHAR(20)   NULL,
    ProgramId   INT           NOT NULL,
    CreatedAt   DATETIME2     NOT NULL
        CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt   DATETIME2     NULL,
    CONSTRAINT CK_Students_NRP
        CHECK (LEN(NRP) = 10 AND NRP NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Students_Email
        CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT FK_Students_Programs
        FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId)
);
GO

/* ---------- 5. Tabel master: Courses (mata kuliah) ---------- */
CREATE TABLE dbo.Courses
(
    CourseId   INT IDENTITY(1,1)
        CONSTRAINT PK_Courses PRIMARY KEY,
    CourseCode VARCHAR(10)  NOT NULL
        CONSTRAINT UQ_Courses_CourseCode UNIQUE,
    CourseName VARCHAR(100) NOT NULL,
    Credits    TINYINT      NOT NULL
        CONSTRAINT CK_Courses_Credits CHECK (Credits BETWEEN 1 AND 6),
    Semester   TINYINT      NOT NULL
        CONSTRAINT CK_Courses_Semester CHECK (Semester BETWEEN 1 AND 8),
    CreatedAt  DATETIME2    NOT NULL
        CONSTRAINT DF_Courses_CreatedAt DEFAULT SYSDATETIME()
);
GO

/* ---------- 6. Tabel transaksi: Enrollments (KRS)
   Tabel penghubung Many-to-Many antara Students dan Courses. ---------- */
CREATE TABLE dbo.Enrollments
(
    EnrollmentId INT IDENTITY(1,1)
        CONSTRAINT PK_Enrollments PRIMARY KEY,
    StudentId    INT        NOT NULL,
    CourseId     INT        NOT NULL,
    AcademicYear CHAR(9)    NOT NULL
        CONSTRAINT CK_Enrollments_AcademicYear
        CHECK (AcademicYear LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]'
               AND CAST(RIGHT(AcademicYear, 4) AS INT) = CAST(LEFT(AcademicYear, 4) AS INT) + 1),
    Term         VARCHAR(5) NOT NULL
        CONSTRAINT CK_Enrollments_Term CHECK (Term IN ('Gasal', 'Genap')),
    Grade        VARCHAR(2) NULL
        CONSTRAINT CK_Enrollments_Grade CHECK (Grade IN ('A', 'AB', 'B', 'BC', 'C', 'D', 'E')),
    EnrolledAt   DATETIME2  NOT NULL
        CONSTRAINT DF_Enrollments_EnrolledAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Enrollments_Students
        FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_Enrollments_Courses
        FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    CONSTRAINT UQ_Enrollments_Student_Course_Period
        UNIQUE (StudentId, CourseId, AcademicYear, Term)
);
GO

/* ---------- 7. Data contoh ---------- */
INSERT INTO dbo.Programs (ProgramCode, ProgramName, Faculty) VALUES
('IF',  'Teknik Informatika',       'Fakultas Teknologi Elektro dan Informatika Cerdas'),
('SI',  'Sistem Informasi',         'Fakultas Teknologi Elektro dan Informatika Cerdas'),
('TK',  'Teknik Komputer',          'Fakultas Teknologi Elektro dan Informatika Cerdas'),
('RKA', 'Rekayasa Kecerdasan Artifisial', 'Fakultas Teknologi Elektro dan Informatika Cerdas');

INSERT INTO dbo.Students (NRP, FullName, Gender, Email, EntryYear, PhoneNumber, ProgramId) VALUES
('5025241001', N'Andi Pratama',      'L', 'andi.pratama@student.its.ac.id',   2024, '081234567801', 1),
('5025241002', N'Bunga Lestari',     'P', 'bunga.lestari@student.its.ac.id',  2024, '081234567802', 1),
('5025241003', N'Caraka Wijaya',     'L', 'caraka.wijaya@student.its.ac.id',  2024, NULL,           1),
('5026241004', N'Dewi Anggraini',    'P', 'dewi.anggraini@student.its.ac.id', 2024, '081234567804', 2),
('5026231005', N'Eko Saputra',       'L', 'eko.saputra@student.its.ac.id',    2023, '081234567805', 2),
('5024231006', N'Fitri Handayani',   'P', 'fitri.handayani@student.its.ac.id',2023, NULL,           3);

INSERT INTO dbo.Courses (CourseCode, CourseName, Credits, Semester) VALUES
('EF234301', 'Pemrograman Berbasis Kerangka Kerja', 3, 5),
('EF234302', 'Basis Data',                          4, 3),
('EF234303', 'Struktur Data',                       4, 2),
('EF234304', 'Matematika Diskrit',                  3, 1),
('EF234305', 'Rekayasa Perangkat Lunak',            3, 4);

INSERT INTO dbo.Enrollments (StudentId, CourseId, AcademicYear, Term, Grade) VALUES
(1, 1, '2026/2027', 'Gasal', NULL),
(1, 2, '2025/2026', 'Gasal', 'A'),
(1, 4, '2024/2025', 'Gasal', 'AB'),
(2, 1, '2026/2027', 'Gasal', NULL),
(2, 4, '2024/2025', 'Gasal', 'A'),
(3, 3, '2024/2025', 'Genap', 'B'),
(4, 2, '2025/2026', 'Gasal', 'BC'),
(5, 5, '2025/2026', 'Genap', 'A');
GO

/* ---------- 8. View gabungan 4 tabel (dipakai aplikasi) ---------- */
CREATE VIEW dbo.vw_EnrollmentDetail
AS
SELECT
    e.EnrollmentId,
    e.StudentId,
    s.NRP,
    s.FullName,
    p.ProgramCode,
    e.CourseId,
    c.CourseCode,
    c.CourseName,
    c.Credits,
    e.AcademicYear,
    e.Term,
    e.Grade,
    CAST(CASE e.Grade
            WHEN 'A'  THEN 4.0 WHEN 'AB' THEN 3.5
            WHEN 'B'  THEN 3.0 WHEN 'BC' THEN 2.5
            WHEN 'C'  THEN 2.0 WHEN 'D'  THEN 1.0
            WHEN 'E'  THEN 0.0
         END AS DECIMAL(3,2)) AS GradePoint,
    e.EnrolledAt
FROM dbo.Enrollments e
JOIN dbo.Students s ON s.StudentId = e.StudentId
JOIN dbo.Programs p ON p.ProgramId = s.ProgramId
JOIN dbo.Courses  c ON c.CourseId  = e.CourseId;
GO

/* ---------- 9. Menampilkan isi tabel ---------- */
SELECT * FROM dbo.Programs;
SELECT * FROM dbo.Students;
SELECT * FROM dbo.Courses;
SELECT * FROM dbo.Enrollments;
SELECT * FROM dbo.vw_EnrollmentDetail ORDER BY NRP, AcademicYear;
GO

/* ---------- 10. Uji constraint (setiap uji DIHARAPKAN GAGAL) ----------
   TRY...CATCH dipakai agar script tetap berjalan sampai akhir. */
BEGIN TRY  -- UNIQUE: NRP kembar
    INSERT INTO dbo.Students (NRP, FullName, Gender, Email, EntryYear, ProgramId)
    VALUES ('5025241001', N'Duplikat', 'L', 'dup@student.its.ac.id', 2024, 1);
END TRY
BEGIN CATCH SELECT 'UNIQUE NRP' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;

BEGIN TRY  -- FOREIGN KEY: ProgramId tidak ada
    INSERT INTO dbo.Students (NRP, FullName, Gender, Email, EntryYear, ProgramId)
    VALUES ('5025249999', N'Prodi Fiktif', 'P', 'fiktif@student.its.ac.id', 2024, 99);
END TRY
BEGIN CATCH SELECT 'FOREIGN KEY ProgramId' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;

BEGIN TRY  -- CHECK: SKS di luar 1..6
    INSERT INTO dbo.Courses (CourseCode, CourseName, Credits, Semester)
    VALUES ('EF999999', 'SKS Tidak Valid', 10, 1);
END TRY
BEGIN CATCH SELECT 'CHECK Credits' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;

BEGIN TRY  -- CHECK: nilai huruf tidak dikenal
    INSERT INTO dbo.Enrollments (StudentId, CourseId, AcademicYear, Term, Grade)
    VALUES (3, 1, '2026/2027', 'Gasal', 'X');
END TRY
BEGIN CATCH SELECT 'CHECK Grade' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;

BEGIN TRY  -- UNIQUE komposit: MK yang sama di periode yang sama
    INSERT INTO dbo.Enrollments (StudentId, CourseId, AcademicYear, Term)
    VALUES (1, 1, '2026/2027', 'Gasal');
END TRY
BEGIN CATCH SELECT 'UNIQUE Enrollment' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;

BEGIN TRY  -- FOREIGN KEY: menghapus mahasiswa yang masih punya KRS
    DELETE FROM dbo.Students WHERE StudentId = 1;
END TRY
BEGIN CATCH SELECT 'FOREIGN KEY saat DELETE' AS Uji, ERROR_NUMBER() AS Kode, ERROR_MESSAGE() AS Pesan; END CATCH;
GO

/* ---------- 11. DEFAULT: kolom CreatedAt dan IsActive terisi otomatis ---------- */
SELECT ProgramCode, IsActive, CreatedAt FROM dbo.Programs;
GO

/* ---------- 12. Query JOIN, GROUP BY, COUNT ---------- */
-- Jumlah mahasiswa per program studi (LEFT JOIN agar prodi tanpa mahasiswa tetap muncul)
SELECT p.ProgramCode, p.ProgramName, COUNT(s.StudentId) AS JumlahMahasiswa
FROM dbo.Programs p
LEFT JOIN dbo.Students s ON s.ProgramId = p.ProgramId
GROUP BY p.ProgramCode, p.ProgramName
ORDER BY JumlahMahasiswa DESC;

-- Jumlah peserta per mata kuliah
SELECT c.CourseCode, c.CourseName, COUNT(e.EnrollmentId) AS JumlahPeserta
FROM dbo.Courses c
LEFT JOIN dbo.Enrollments e ON e.CourseId = c.CourseId
GROUP BY c.CourseCode, c.CourseName
ORDER BY JumlahPeserta DESC;

-- Total SKS diambil dan IPK per mahasiswa (IPK hanya dari MK yang sudah bernilai)
SELECT
    NRP, FullName,
    COUNT(*)     AS JumlahMK,
    SUM(Credits) AS TotalSKS,
    CAST(SUM(GradePoint * Credits) / NULLIF(SUM(CASE WHEN Grade IS NOT NULL THEN Credits END), 0)
         AS DECIMAL(3,2)) AS IPK
FROM dbo.vw_EnrollmentDetail
GROUP BY NRP, FullName
ORDER BY NRP;
GO
