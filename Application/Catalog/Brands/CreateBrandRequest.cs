using System;

namespace Application.Catalog.Brands;

public class CreateBrandRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateBrandRequestValidator : CustomValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator(IReadRepository<Brand> repository, IStringLocalizer<CreateBrandRequestValidator> localizer) =>
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(75)
            .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new BrandByNameSpec(name), ct) is null)
                .WithMessage((_, name) => string.Format(localizer["brand.alreadyexists"], name));
}

public class CreateBrandRequestHandler : IRequestHandler<CreateBrandRequest, Guid>
{
    // Add Domain Events automatically by using IRepositoryWithEvents
    private readonly IRepository<Brand> _repository;
    private readonly IValidator<CreateBrandRequest> _validator;

    public CreateBrandRequestHandler(IValidator<CreateBrandRequest> validator, IRepository<Brand> repository) => (_validator, _repository) = (validator, repository);

    public async Task<Guid> Handle(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        ValidationResult result = await _validator.ValidateAsync(request);
        if (!result.IsValid) throw new ValidationException(result.Errors);

        var brand = new Brand(request.Name, request.Description);

        await _repository.AddAsync(brand, cancellationToken);

        return brand.Id;
    }
}