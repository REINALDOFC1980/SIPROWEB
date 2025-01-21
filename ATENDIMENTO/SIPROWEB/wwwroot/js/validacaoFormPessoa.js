function ValidacaoFormPessoa(selector) {
    $(selector).validate({
        lang: 'pt_BR',
        rules: {
            cpf: {
                required: true,
                minlength: 14, // Ajuste aqui para 11 caracteres sem contar os separadores de ponto e traço
                validarCPF: true // Usando a regra de validação de CPF personalizada
            },
            Rec_CNH_Condutor: {
                /*  required: true,*/
                minlength: 11
            },
            PES_DT_Validade: {
                /*         required: true,*/
                //validDate: true,
                verificaDataVencimento: true,
            },
            PES_UFCNH: {
                /*          required: true,*/
                validarUF: true
            },
            PES_UF: {
                required: true,
                validarUF: true
            },
            PES_Celular: {
                minlength: 14

            },
            PES_Telefone: {
                minlength: 14

            },
            PES_Email: {
                required: true,
                email: true
            },
            Age_Cod_Assunto: {
                selectValido: "0"
            }

        },
        messages: {
            cpf: {
                required: "Por favor, insira o CPF.",
                minlength: "Por favor, insira um CPF válido.",
                validarCPF: "CPF inválido"
            },
            Rec_CNH_Condutor: {
                required: "Por favor, insira a CNH.",
                minlength: "Por favor, insira uma CHN válida."
            },
            PES_DT_Validade: {
                required: "Por favor, insira a data de validade.",
                validDate: "Por favor, insira uma data válida.",
            },
            PES_UFCNH: {
                required: "Por favor, insira a UF da CNH.",
                validarUF: "Por favor, insira uma UF válida."
            },
            PES_UF: {
                required: "Por favor, insira a UF.",
                validarUF: "Por favor, insira uma UF válida."
            },
            PES_Celular: {
                minlength: "Minimo de 14 caracteres."
            },
            PES_Telefone: {
                minlength: "Minimo de 14 caracteres ou vazio."
            },
            PES_Email: {
                required: "Por favor, insira seu e-mail.",
                email: "Por favor, insira um e-mail válido."
            },
            Age_Cod_Assunto: { // Mensagem de erro para o campo select
                required: "Por favor, selecione uma opção."
            }
        },

    });

    // método de validação de CPF
    $.validator.addMethod("validarCPF", function (value, element) {
        return validarCPF(value); // Chama a função validarCPF
    }, "Por favor, insira um CPF válido.");

    // Adicionando uma nova regra de validação personalizada para verificar a data de vencimento
    $.validator.addMethod("verificaDataVencimento", function (value, element) {
        // Use a função verificarDataVencimento para verificar se a data de vencimento é válida
        return !verificarDataVencimento(value);
    }, "CNH fora do prazo!");

    // Adicionando a regra de validação de data personalizada
    $.validator.addMethod("validDate", function (value, element) {
        // Use o regex para verificar se a data está no formato correto (dd/mm/yyyy)
        if (!/^\d{2}\/\d{2}\/\d{4}$/.test(value))
            return false;

        // Extrair dia, mês e ano
        var parts = value.split('/');
        var day = parseInt(parts[0], 10);
        var month = parseInt(parts[1], 10);
        var year = parseInt(parts[2], 10);

        // Verificar se é uma data válida
        var date = new Date(year, month - 1, day);
        return (
            date.getFullYear() === year &&
            date.getMonth() + 1 === month &&
            date.getDate() === day
        );
    }, "Por favor, insira uma data válida.");

    // Adicionando a regra de validação de UF personalizada
    $.validator.addMethod("validarUF", function (value, element) {
        // Lista de UFs do Brasil
        var ufs = ["","AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"];

        // Convertendo o valor para maiúsculas
        value = value.toUpperCase();

        // Verificando se a UF está na lista
        return ufs.indexOf(value) !== -1;
    }, "Por favor, insira uma UF válida.");


    // Função para validar se um select foi selecionado
    $.validator.addMethod("selectValido", function (value, element, arg) {
        var isValid = arg !== value;
        // Se o campo não for válido, foca no elemento
        if (!isValid) {
            $(element).focus();
        }
        return isValid;
    }, "Por favor, selecione uma opção.");

}