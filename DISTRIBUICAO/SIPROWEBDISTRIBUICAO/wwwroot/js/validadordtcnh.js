function verificarDataVencimento(data) {
    // Separar os componentes da data
    var partesData = data.split('/');
    var dia = parseInt(partesData[0], 10);
    var mes = parseInt(partesData[1], 10) - 1; // Subtrai 1 porque os meses em JavaScript são baseados em zero
    var ano = parseInt(partesData[2], 10);

    // Construir um objeto Date com a data fornecida
    var dataVencimento = new Date(ano, mes, dia);

    // Obter a data atual
    var hoje = new Date();

    // Verificar se a data de vencimento é menor ou igual à data atual
    if (dataVencimento <= hoje) {
        return true;
    } else {
        return false;
    }
}
