-- Database untuk aplikasi Absensi Mahasiswa (WPF + MVVM + MySQL)
-- Jalankan di MySQL / MariaDB (XAMPP, Laragon, MySQL Workbench, dll.)

CREATE DATABASE IF NOT EXISTS absensi_db;
USE absensi_db;

-- Tabel master mahasiswa
CREATE TABLE IF NOT EXISTS students (
    id       INT AUTO_INCREMENT PRIMARY KEY,
    nrp      VARCHAR(10)  NOT NULL UNIQUE,
    nama     VARCHAR(100) NOT NULL,
    jurusan  VARCHAR(60)  NOT NULL,
    email    VARCHAR(100) NOT NULL
);

-- Tabel transaksi absensi
-- UNIQUE (student_id, mata_kuliah, tanggal) mencegah absensi ganda di level database
CREATE TABLE IF NOT EXISTS attendances (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    student_id   INT          NOT NULL,
    mata_kuliah  VARCHAR(80)  NOT NULL,
    tanggal      DATE         NOT NULL,
    status       ENUM('Hadir','Izin','Sakit','Alpha') NOT NULL,
    keterangan   VARCHAR(200) NOT NULL DEFAULT '',
    CONSTRAINT fk_attendance_student
        FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE CASCADE,
    CONSTRAINT uq_attendance UNIQUE (student_id, mata_kuliah, tanggal)
);

-- Data contoh (opsional)
INSERT INTO students (nrp, nama, jurusan, email) VALUES
('5025241001', 'Andi Pratama',   'Teknik Informatika',  'andi@student.its.ac.id'),
('5025241002', 'Budi Santoso',   'Teknik Informatika',  'budi@student.its.ac.id'),
('5026241003', 'Citra Lestari',  'Sistem Informasi',    'citra@student.its.ac.id');

INSERT INTO attendances (student_id, mata_kuliah, tanggal, status, keterangan) VALUES
(1, 'Pemrograman Berbasis Kerangka Kerja', CURDATE(), 'Hadir', ''),
(2, 'Pemrograman Berbasis Kerangka Kerja', CURDATE(), 'Sakit', 'Demam, surat dokter menyusul'),
(3, 'Pemrograman Berbasis Kerangka Kerja', CURDATE(), 'Izin',  'Lomba');
