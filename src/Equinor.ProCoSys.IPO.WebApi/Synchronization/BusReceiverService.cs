using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Text.Json;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Equinor.ProCoSys.BusReceiver.Topics;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public class BusReceiverService : IBusReceiverService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPlantSetter _plantSetter;
        private readonly IUnitOfWork _unitOfWork;

        public BusReceiverService(IInvitationRepository invitationRepository, IPlantSetter plantSetter, IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _plantSetter = plantSetter;
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessMessageAsync(PcsTopic pcsTopic, Message message, CancellationToken token)
        {
            var messageJson = Encoding.UTF8.GetString(message.Body);
            switch (pcsTopic)
            {
                case PcsTopic.Project:
                    var projectEvent = JsonSerializer.Deserialize<ProjectTopic>(messageJson);

                    _plantSetter.SetPlant(projectEvent.ProjectSchema);
                    _invitationRepository.UpdateProjectOnInvitations(projectEvent.ProjectName, projectEvent.Description);
                    break;
                case PcsTopic.CommPkg:
                    var commPkgEvent = JsonSerializer.Deserialize<CommPkgTopic>(messageJson);

                    _plantSetter.SetPlant(commPkgEvent.ProjectSchema);
                    _invitationRepository.UpdateCommPkgOnInvitations(commPkgEvent.ProjectName, commPkgEvent.CommPkgNo, commPkgEvent.Description);
                    break;
                case PcsTopic.McPkg:
                    var mcPkgEvent = JsonSerializer.Deserialize<McPkgTopic>(messageJson);

                    _plantSetter.SetPlant(mcPkgEvent.ProjectSchema);
                    _invitationRepository.UpdateMcPkgOnInvitations(mcPkgEvent.ProjectName, mcPkgEvent.McPkgNo, mcPkgEvent.Description);
                    break;
            }
            await _unitOfWork.SaveChangesAsync(token);
        }
    }
}
