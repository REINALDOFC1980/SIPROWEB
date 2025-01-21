
var duplicidade = '@ViewBag.Analise.Duplicidade';
var situacaoAIT = '@ViewBag.Analise.SituacaoAIT';
var intepestividade = '@ViewBag.Analise.Intepestividade';
var codservico = $('#Rec_Id_Servico').val();

var inputElement = document.querySelector('#Rec_NAI_Condutor');
var radioElement = document.getElementById('optionsRadios6');
if (intepestividade == "Sim") {
    inputElement.classList.add('text-danger');
    radioElement.checked = true;
}



if ((situacaoAIT == "NIP" && codservico == 1) || (duplicidade > 0) || (intepestividade == "Sim")) {
    $('#ModalErro').modal({
        backdrop: 'static',
        keyboard: false
    });
} else {
    $('#Rec_Cpf_Condutor').focus();
}


$('#btnVoltar').on('click', function () {
    $('#ModalErro').modal('hide');
    $('#Rec_Cpf_Condutor').focus();

    if ((situacaoAIT == "NIP" && codservico == 1) || (duplicidade > 0)) {
        window.location.href = '@Url.Action("Index", "Home")';
    }
});


$('#btnbuscar_condutor').on('click', function () {
    $('#Rec_Cpf_Condutor').blur();
});


$.ajax({
    url: '/Atendimento/BuscarCondutor', // Use a URL diretamente, não é necessário usar '@Url.Action'
    type: 'GET',
    data: { cpf: cpf }, // Envie todos os dados do formulário
    success: function (response) {
        // Manipule a resposta (response) aqui
    },
    error: function (xhr, status, error) {
        // Tratar erros de requisição
    }
});