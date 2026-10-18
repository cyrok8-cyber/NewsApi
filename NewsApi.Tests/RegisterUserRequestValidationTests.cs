using System.ComponentModel.DataAnnotations;
using NewsApi.DTOs.Requests;
using Xunit;

namespace NewsApi.Tests
{
    public class RegisterUserRequestValidationTests
    {
        private static List<ValidationResult> Validate(RegisterUserRequest request)
        {
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(request, context, results, validateAllProperties: true);
            return results;
        }

        private static RegisterUserRequest CreateValidRequest()
        {
            return new RegisterUserRequest
            {
                Username = "testuser",
                Password = "Qwerty123",
                Name = "Иван",
                Surname = "Иванов",
                ParentName = "Иванович",
                Role = "subscriber"
            };
        }

        // ---------- Позитивный сценарий: обычное ФИО ----------
        [Fact]
        public void Validate_PlainName_IsValid()
        {
            var request = CreateValidRequest();

            var results = Validate(request);

            Assert.Empty(results);
        }

        // ---------- Позитивный сценарий: двойная фамилия через дефис ----------
        [Fact]
        public void Validate_HyphenatedSurname_IsValid()
        {
            var request = CreateValidRequest();
            request.Surname = "Петров-Водкин";

            var results = Validate(request);

            Assert.Empty(results);
        }

        // ---------- Негативный сценарий: цифры в имени ----------
        [Fact]
        public void Validate_NameWithDigits_IsInvalid()
        {
            var request = CreateValidRequest();
            request.Name = "Иван123";

            var results = Validate(request);

            Assert.NotEmpty(results);
        }

        // ---------- Негативный сценарий: недопустимый спецсимвол (не дефис) ----------
        [Fact]
        public void Validate_SurnameWithInvalidSpecialCharacter_IsInvalid()
        {
            var request = CreateValidRequest();
            request.Surname = "Иванов_Петров";

            var results = Validate(request);

            Assert.NotEmpty(results);
        }

        // ---------- Негативный сценарий: пробел в отчестве ----------
        [Fact]
        public void Validate_ParentNameWithSpace_IsInvalid()
        {
            var request = CreateValidRequest();
            request.ParentName = "Иванович Петрович";

            var results = Validate(request);

            Assert.NotEmpty(results);
        }

        // ---------- Негативный сценарий: латиница вместо кириллицы ----------
        [Fact]
        public void Validate_LatinLetters_IsInvalid()
        {
            var request = CreateValidRequest();
            request.Name = "Ivan";

            var results = Validate(request);

            Assert.NotEmpty(results);
        }
    }
}