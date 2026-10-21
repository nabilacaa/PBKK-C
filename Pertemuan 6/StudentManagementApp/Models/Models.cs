using System;

namespace StudentManagementApp.Models;

/// <summary>Satu baris tabel Programs.</summary>
public class StudyProgram
{
    public int ProgramId { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;

    public string Display => $"{ProgramCode} - {ProgramName}";
}

/// <summary>Satu baris tabel Students, ditambah nama prodi hasil JOIN.</summary>
public class Student
{
    public int StudentId { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "L";
    public string Email { get; set; } = string.Empty;
    public short EntryYear { get; set; }
    public string? PhoneNumber { get; set; }
    public int ProgramId { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string GenderText => Gender == "P" ? "Perempuan" : "Laki-laki";
    public string Display => $"{Nrp} - {FullName}";
}

/// <summary>Satu baris tabel Courses.</summary>
public class Course
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public byte Credits { get; set; }

    public string Display => $"{CourseCode} - {CourseName} ({Credits} SKS)";
}

/// <summary>Satu baris view vw_EnrollmentDetail.</summary>
public class EnrollmentDetail
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public byte Credits { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public decimal? GradePoint { get; set; }

    public string Period => $"{AcademicYear} {Term}";
    public string GradeText => Grade ?? "-";
}

/// <summary>Hasil query GROUP BY jumlah mahasiswa per prodi.</summary>
public class ProgramSummary
{
    public string ProgramCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}
