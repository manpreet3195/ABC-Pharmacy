namespace ABCPharmacy.Validators;

public interface IValidator<in T>
{
    IReadOnlyList<string> Validate(T instance);
}
