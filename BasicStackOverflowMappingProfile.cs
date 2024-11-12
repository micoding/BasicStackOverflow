using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Models;

namespace BasicStackOverflow;

public class BasicStackOverflowMappingProfile : Profile
{
    public BasicStackOverflowMappingProfile()
    {
        CreateMap<Comment, CommentDTO>().ForMember(x => x.Username, y=> y.MapFrom(z => z.Author.Username));
        CreateMap<Answer, AnswerDTO>().ForMember(x => x.Username, y=> y.MapFrom(z =>z.Author.Username))
            .ForMember(x => x.Comments, y=> y.MapFrom(z => z.Comments));
        CreateMap<Tag, TagDTO>();
        CreateMap<Question, QuestionDTO>().ForMember(x => x.Username, y=> y.MapFrom(z => z.Author.Username))
            .ForMember(q => q.Comments, y=> y.MapFrom(z => z.Comments))
            .ForMember(q => q.Tags, y=> y.MapFrom(z => z.Tags))
            .ForMember(q => q.Answers, y=> y.MapFrom(z => z.Answers));
    }
}