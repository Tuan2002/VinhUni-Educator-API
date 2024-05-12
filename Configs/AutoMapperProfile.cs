using AutoMapper;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Configs
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RefreshToken, RefreshTokenModel>();
            CreateMap<ApplicationUser, PublicUserModel>();
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<Course, CourseViewModel>();
            CreateMap<Major, MajorViewModel>();
            CreateMap<TrainingProgram, ProgramViewModel>()
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<PrimaryClass, ClassViewModel>()
            .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.Program.ProgramName))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<Organization, OrganizationViewModel>();

            // Mapper for Student
            CreateMap<Student, StudentViewModel>()
            .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.PrimaryClass != null ? src.PrimaryClass.ClassName : null))
            .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.TrainingProgram != null ? src.TrainingProgram.ProgramName : null))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));
            CreateMap<StudentSyncModel, ImportStudentModel>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.code))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.ho))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.ten))
            .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ngaySinh)))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => ConvertGender.ConvertToInt(src.gioiTinh)))
            .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.idLopHanhChinh))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.idKhoaHoc))
            .ForMember(dest => dest.ProgramCode, opt => opt.MapFrom(src => src.idNganh))
            .ForMember(dest => dest.SSOId, opt => opt.MapFrom(src => int.Parse(src.userId)));
            CreateMap<ImportStudentModel, ImportStudentViewModel>();

            // Mapper for Teacher
            CreateMap<Teacher, TeacherViewModel>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.OrganizationName : null));
            CreateMap<TeacherSyncModel, ImportTeacherModel>()
            .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => src.hS_ID))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.hS_Ho))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.hS_Ten))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.hS_GioiTinh))
            .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ngaySinh)))
            .ForMember(dest => dest.OrganizationCode, opt => opt.MapFrom(src => src.dV_ID_GiangDay))
            .ForMember(dest => dest.SSOId, opt => opt.MapFrom(src => int.Parse(src.userId)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.hS_Email));
            CreateMap<ImportTeacherModel, ImportTeacherViewModel>();

            // Mapper for SchoolYear
            CreateMap<SchoolYear, SchoolYearViewModel>();
            CreateMap<Semester, SemesterViewModel>()
            .ForMember(dest => dest.SchoolYearName, opt => opt.MapFrom(src => src.SchoolYear.SchoolYearName));

            // Mapper for Module
            CreateMap<Module, ModuleViewModel>()
            .ForMember(dest => dest.ApplyYearName, opt => opt.MapFrom(src => src.ApplyYear != null ? src.ApplyYear.SchoolYearName : null));

            // Mapper for ModuleClass
            CreateMap<ModuleClass, ClassModuleViewModel>()
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module.ModuleName))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FirstName + " " + src.Teacher.LastName))
            .ForMember(dest => dest.CurrentStudents, opt => opt.MapFrom(src => src.ModuleClassStudents.Count));

            // Mapper for category
            CreateMap<Category, CategoryViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.IsShared, opt => opt.MapFrom(src => src.Owner.UserId != src.CreatedById))
            .ForMember(dest => dest.IsSharing, opt => opt.MapFrom(src => src.ShareCategories.Count > 0))
            .ForMember(dest => dest.SharedAt, opt => opt.MapFrom(src => src.ShareCategories.Where(sc => sc.CategoryId == src.Id).Select(sc => sc.SharedAt).FirstOrDefault()))
            .ForMember(dest => dest.ShareUntil, opt => opt.MapFrom(src => src.ShareCategories.Where(sc => sc.CategoryId == src.Id).Select(sc => sc.SharedUntil).FirstOrDefault()));

            // Mapper for shared category
            CreateMap<SharedCategory, SharedCategoryViewModel>()
            .ForMember(dest => dest.SharedAt, opt => opt.MapFrom(src => src.SharedAt))
            .ForMember(dest => dest.SharedUntil, opt => opt.MapFrom(src => src.SharedUntil))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.ViewerId))
            .ForMember(dest => dest.TeacherCode, opt => opt.MapFrom(src => src.Viewer.TeacherCode))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Viewer.FirstName + " " + src.Viewer.LastName));

            // Mapper for question kit
            CreateMap<QuestionKit, QuestionKitViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.ModifiedByName, opt => opt.MapFrom(src => src.ModifiedBy != null ? src.ModifiedBy.FirstName + " " + src.ModifiedBy.LastName : null))
            .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.Questions.Count));

            // Mapper for question
            CreateMap<CreateQuestionModel, CreateQuestionResult>();
            CreateMap<Question, QuestionViewModel>()
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers.Select(a => new QuestionAnswerViewModel
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                AnswerContent = a.AnswerContent,
                AnswerImage = a.AnswerImage,
                IsCorrect = a.IsCorrect
            }).ToList()));
            CreateMap<Question, StudentQuestionViewModel>()
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers.Select(a => new QuestionAnswerViewModel
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                AnswerContent = a.AnswerContent,
                AnswerImage = a.AnswerImage,
            }).ToList()));

            // Mapper for exam
            CreateMap<Exam, ExamViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.ExamQuestions.Count));

            // Mapper for exam season
            CreateMap<ExamSeason, ExamSeasonViewModel>();
            CreateMap<ExamSeason, StudentSeasonViewModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName));
            CreateMap<ExamSeason, ExamSeasonDetailModel>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.ExamName, opt => opt.MapFrom(src => src.Exam.ExamName))
            .ForMember(dest => dest.TotalClasses, opt => opt.MapFrom(src => src.AssignedClasses.Count));

            // Mapper for exam assigned class
            CreateMap<ExamAssignedClass, AssignedClassViewModel>()
            .ForMember(dest => dest.ModuleClassName, opt => opt.MapFrom(src => src.ModuleClass.ModuleClassName))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.ModuleClass.Teacher.FirstName + " " + src.ModuleClass.Teacher.LastName));

            // Mapper for exam participant
            CreateMap<ExamParticipant, ExamParticipantViewModel>()
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student.StudentCode))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.GetFullName()))
            .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt))
            .ForMember(dest => dest.TotalTurn, opt => opt.MapFrom(src => src.ExamTurns.Count))
            .ForMember(dest => dest.TotalFinishedTurn, opt => opt.MapFrom(src => src.ExamTurns.Count(t => t.IsFinished)))
            .ForMember(dest => dest.IsAllTurnFinished, opt => opt.MapFrom(src => src.ExamTurns.All(t => t.IsFinished)))
            .ForMember(dest => dest.LastTurnFinishedAt, opt => opt.MapFrom(src => src.ExamTurns.Where(t => t.IsFinished).Select(t => t.CompletedAt).LastOrDefault()))
            .ForMember(dest => dest.HighestPoint, opt => opt.MapFrom(src => src.ExamTurns.Where(t => t.IsFinished).Select(t => t.ExamResult != null ? t.ExamResult.TotalPoint : 0).Max()))
            .ForMember(dest => dest.AveragePoint, opt => opt.MapFrom(src => src.ExamTurns.Where(t => t.IsFinished).Select(t => t.ExamResult != null ? t.ExamResult.TotalPoint : 0).Average()));

            // Mapper for exam turn
            CreateMap<ExamTurn, StudentExamTurnModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExamSeasonCode, opt => opt.MapFrom(src => src.ExamSeason.SeasonCode))
            .ForMember(dest => dest.ExamSeasonName, opt => opt.MapFrom(src => src.ExamSeason.SeasonName))
            .ForMember(dest => dest.TurnNumber, opt => opt.MapFrom(src => src.TurnNumber))
            .ForMember(dest => dest.StartAt, opt => opt.MapFrom(src => src.StartAt))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
            .ForMember(dest => dest.IsFinished, opt => opt.MapFrom(src => src.IsFinished))
            .ForMember(dest => dest.AllowContinue, opt => opt.MapFrom(src => !src.IsFinished && src.StartAt.AddMinutes(src.ExamSeason.DurationInMinutes) > DateTime.UtcNow))
            .ForMember(dest => dest.AllowViewResult, opt => opt.MapFrom(src => src.IsFinished && src.ExamResult != null && src.ExamSeason.ShowResult))
            .ForMember(dest => dest.TotalPoint, opt => opt.MapFrom(src => src.ExamResult != null ? src.ExamResult.TotalPoint : 0));

            // Mapper for exam result
            CreateMap<ExamResult, ExamResultViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExamSeasonCode, opt => opt.MapFrom(src => src.ExamTurn.ExamSeason.SeasonCode))
            .ForMember(dest => dest.ExamSeasonName, opt => opt.MapFrom(src => src.ExamTurn.ExamSeason.SeasonName))
            .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.ExamTurn.ExamParticipant.Student.StudentCode))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.ExamTurn.ExamParticipant.Student.GetFullName()))
            .ForMember(dest => dest.TurnId, opt => opt.MapFrom(src => src.ExamTurn.Id))
            .ForMember(dest => dest.TurnNumber, opt => opt.MapFrom(src => src.ExamTurn.TurnNumber))
            .ForMember(dest => dest.StartAt, opt => opt.MapFrom(src => src.ExamTurn.StartAt))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.ExamTurn.CompletedAt))
            .ForMember(dest => dest.CorrectAnswers, opt => opt.MapFrom(src => src.ExamResultDetails.Count(d => d.IsCorrect)))
            .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.ExamTurn.ExamSeason.Exam.ExamQuestions.Count))
            .ForMember(dest => dest.TotalPoint, opt => opt.MapFrom(src => src.TotalPoint));

            // Mapper for exam result detail
            CreateMap<ExamResultDetail, StudentQuestionResult>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.QuestionId))
            .ForMember(dest => dest.QuestionContent, opt => opt.MapFrom(src => src.Question.QuestionContent))
            .ForMember(dest => dest.QuestionNote, opt => opt.MapFrom(src => src.Question.QuestionNote))
            .ForMember(dest => dest.QuestionImages, opt => opt.MapFrom(src => src.Question.QuestionImages))
            .ForMember(dest => dest.IsMultipleChoice, opt => opt.MapFrom(src => src.Question.IsMultipleChoice))
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Question.Answers.Select(a => new QuestionAnswerResult
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                AnswerContent = a.AnswerContent,
                AnswerImage = a.AnswerImage,
                IsCorrect = a.IsCorrect,
                IsSelected = src.SelectedAnswerId == a.Id
            }).ToList()));

        }
    }
}