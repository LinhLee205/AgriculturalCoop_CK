using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.UseCases.Members
{
    public interface IViewMembersUseCase
    {
        Task<IEnumerable<Member>> ExecuteAsync();
    }

    public class ViewMembersUseCase : IViewMembersUseCase
    {
        private readonly IMemberRepository _memberRepository;
        public ViewMembersUseCase(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<IEnumerable<Member>> ExecuteAsync()
        {
            return await _memberRepository.GetAllMembersAsync();
        }
    }

    public interface IAddMemberUseCase
    {
        Task ExecuteAsync(Member member);
    }

    public class AddMemberUseCase : IAddMemberUseCase
    {
        private readonly IMemberRepository _memberRepository;
        public AddMemberUseCase(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task ExecuteAsync(Member member)
        {
            await _memberRepository.AddMemberAsync(member);
        }
    }
}
