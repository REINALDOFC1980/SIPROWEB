namespace SIPROSHARED.Models
{

    public class ResultGetCnhPorCpf
    {
        public string? nome { get; set; }
        public string? nomeMae { get; set; }
        public string? cpf { get; set; }
        public string? dataNascimento { get; set; }
        public string? categoria { get; set; }
        public string? nrRegistro { get; set; }
        public string? dataDeValidadeCnh { get; set; }
        public string? ufCnh { get; set; }

    }

    public class ApiResponse
    {
        public bool isOk { get; set; }
        public List<ResultGetCnhPorCpf>? items { get; set; }
    }
}


