// Services/TeacherAccessService.cs
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;

namespace SchoolManager.Grades.Services
{
    public interface ITeacherAccessService
    {
        /// <summary>Valida que el profesor es dueño de este TeacherSubjectGroup</summary>
        Task<bool> OwnsTeacherSubjectGroupAsync(int teacherId, int tsgId);

        /// <summary>Valida que el profesor tiene asignado este grupo+materia</summary>
        Task<bool> OwnsGroupSubjectAsync(int teacherId, int groupId, int subjectId);

        /// <summary>Valida que el profesor es dueño de esta calificación (por gradeId)</summary>
        Task<bool> OwnsGradeAsync(int teacherId, int gradeId);

        /// <summary>Valida que el profesor es dueño de esta calificación final</summary>
        Task<bool> OwnsFinalGradeAsync(int teacherId, int finalGradeId);

        /// <summary>Valida que el profesor es dueño de esta recuperación</summary>
        Task<bool> OwnsRecoveryAsync(int teacherId, int recoveryId);

        /// <summary>Valida que el profesor es dueño de este extraordinario</summary>
        Task<bool> OwnsExtraordinaryAsync(int teacherId, int extraordinaryId);
    }

    public class TeacherAccessService : ITeacherAccessService
    {
        private readonly AppDbContext _context;

        public TeacherAccessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> OwnsTeacherSubjectGroupAsync(int teacherId, int tsgId)
        {
            return await _context.grades_TeacherSubjectGroups
                .AnyAsync(tsg => tsg.TeacherSubjectGroupId == tsgId
                              && tsg.TeacherSubject.TeacherId == teacherId);
        }

        public async Task<bool> OwnsGroupSubjectAsync(int teacherId, int groupId, int subjectId)
        {
            return await _context.grades_TeacherSubjectGroups
                .AnyAsync(tsg => tsg.GroupId == groupId
                              && tsg.TeacherSubject.SubjectId == subjectId
                              && tsg.TeacherSubject.TeacherId == teacherId);
        }

        public async Task<bool> OwnsGradeAsync(int teacherId, int gradeId)
        {
            // grade → SubjectUnit → Subject → TeacherSubject(TeacherId)
            // grade → Group → TeacherSubjectGroup → TeacherSubject(TeacherId)
            var grade = await _context.grades_Grades
                .Include(g => g.SubjectUnit)
                .FirstOrDefaultAsync(g => g.GradeId == gradeId);

            if (grade == null) return false;

            return await _context.grades_TeacherSubjectGroups
                .AnyAsync(tsg => tsg.GroupId == grade.GroupId
                              && tsg.TeacherSubject.SubjectId == grade.SubjectUnit.SubjectId
                              && tsg.TeacherSubject.TeacherId == teacherId);
        }

        public async Task<bool> OwnsFinalGradeAsync(int teacherId, int finalGradeId)
        {
            var fg = await _context.grades_FinalGrades
                .FirstOrDefaultAsync(f => f.FinalGradeId == finalGradeId);

            if (fg == null) return false;

            return await OwnsGroupSubjectAsync(teacherId, fg.GroupId, fg.SubjectId);
        }

        public async Task<bool> OwnsRecoveryAsync(int teacherId, int recoveryId)
        {
            var recovery = await _context.grades_UnitRecoveries
                .FirstOrDefaultAsync(r => r.UnitRecoveryId == recoveryId);

            if (recovery == null) return false;

            return await OwnsGradeAsync(teacherId, recovery.GradeId);
        }

        public async Task<bool> OwnsExtraordinaryAsync(int teacherId, int extraordinaryId)
        {
            var extra = await _context.grades_ExtraordinaryGrades
                .FirstOrDefaultAsync(e => e.ExtraordinaryGradeId == extraordinaryId);

            if (extra == null) return false;

            return await OwnsFinalGradeAsync(teacherId, extra.FinalGradeId);
        }
    }
}