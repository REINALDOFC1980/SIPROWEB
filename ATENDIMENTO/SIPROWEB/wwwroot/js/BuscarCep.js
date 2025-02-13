function BuscarCep(cep) {


    var cep = cep.replace(/\D/g, '');

    if (cep !== "") {

        //Expressão regular para validar o CEP.
        var validacep = /^[0-9]{8}$/;

        if (cep.length < 8) {
            Notiflix.Loading.Remove();
            Removeiconeerro(".error-cep");  
            Addiconeerro(".error-cep", " CEP inválido!"); // Exibe mensagem de erro
            //  Notiflix.Loading.Remove();
            $('#PES_EndCEP').focus(); // Foca no campo CEP

        }
        else {
            Removeiconeerro(".error-cep");
        }

        //Valida o formato do CEP.
        if (validacep.test(cep)) {

            //Preenche os campos com "..." enquanto consulta webservice.
            Removeiconeerro(".error-cep");  
            $("#PES_EndLogradouro").val("");
            $("#PES_EndBairro").val("");
            $("#PES_EndNumero").val("");
            $("#PES_Municipio").val("");
            $("#PES_UF").val("");
            $("#PES_EndComplemento").val("");


            //Consulta o webservice viacep.com.br/
            $.getJSON("https://viacep.com.br/ws/" + cep + "/json/?callback=?",
                function (dados) {

                    if (!("erro" in dados)) {
                        //Atualiza os campos com os valores da consulta.

                        $("#PES_EndLogradouro").val(dados.logradouro);
                        $("#PES_EndBairro").val(dados.bairro);
                        $("#PES_Municipio").val(dados.localidade);
                        $("#PES_UF").val(dados.uf);


                        if ($("#PES_EndLogradouro").val() !== "") {
                            $('#PES_EndLogradouro').prop('readonly', true);
                            $('#PES_EndBairro').prop('readonly', true);
                            $('#PES_Municipio').prop('readonly', true);
                            $('#PES_UF').prop('readonly', true);
                            $("#PES_EndNumero").focus();
                        }
                        else {
                            $('#PES_EndLogradouro').prop('readonly', false);
                            $('#PES_EndBairro').prop('readonly', false);
                            $('#PES_EndLogradouro').focus();
                        }

                        Removeiconeerro(".error-cep");  

                        Notiflix.Loading.Remove();
                    }
                    else {
                        Notiflix.Loading.Remove();
                        Addiconeerro(".error-cep"," CEP inválido!");//add um icone antes da imagem  
               /*         $('.error-cep').text(' CEP inválido!');*/
                        $('#PES_EndCEP').focus();
                    
                       // return false;

                    }
                });
        } //end if.
        else {
            Addiconeerro(".error-cep", " CEP inválido!");
            $('#PES_EndCEP').focus();
              Notiflix.Loading.Remove();
            return false;
        }
    } //end if.
    else {
        //cep sem valor, limpa formulário.
        $("#PES_EndLogradouro").val("");
        $("#PES_EndBairro").val("");
        $("#PES_EndNumero").val("");
        $("#PES_Municipio").val("");
        $("#PES_UF").val("");
        $("#PES_EndComplemento").val("");
        $('#PES_EndCEP').focus();
         Notiflix.Loading.Remove();

    }

};