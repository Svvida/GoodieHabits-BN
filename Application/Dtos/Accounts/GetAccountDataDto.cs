using Application.Dtos.Labels;

namespace Application.Dtos.Accounts
{
    public class GetAccountDataDto
    {
        public ICollection<GetQuestLabelDto> QuestsLabels { get; set; } = [];
    }
}
