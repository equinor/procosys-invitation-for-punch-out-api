﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson
{
    public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Result<Unit>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePersonCommandHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            var person = await _personRepository.GetByOidAsync(request.Oid);

            if (person == null)
            {
                person = new Person(request.Oid, request.FirstName, request.LastName, request.UserName, request.Email);
                _personRepository.Add(person);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
