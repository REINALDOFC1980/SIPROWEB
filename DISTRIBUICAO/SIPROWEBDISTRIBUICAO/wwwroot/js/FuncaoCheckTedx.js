//function validarCheckBoxes() {
//    // Selecione todos os checkboxes com a classe documentoCheckbox
//    var checkboxes = document.querySelectorAll('.documentoCheckbox');
//    var peloMenosUmMarcado = false;

//    // Iterar sobre os checkboxes e verificar se pelo menos um está marcado
//    for (var i = 0; i < checkboxes.length; i++) {
//        if (checkboxes[i].checked) {
//            peloMenosUmMarcado = true;
//            break; // Se pelo menos um estiver marcado, não é necessário continuar verificando
//        }
//    }

//    // Retornar verdadeiro se pelo menos um estiver marcado, caso contrário, exibir uma mensagem de alerta e retornar falso
//    if (peloMenosUmMarcado) {
//        return true;
//    } else {
//        alert("Por favor, selecione pelo menos um documento.");
//        return false;
//    }
//}

function validarCheckBoxes() {
    // Selecione todos os checkboxes com a classe documentoCheckbox
    var checkboxes = $('.documentoCheckbox');
    var peloMenosUmMarcado = false;

    // Iterar sobre os checkboxes e verificar se pelo menos um está marcado
    checkboxes.each(function () {
        if ($(this).is(':checked')) {
            peloMenosUmMarcado = true;
            return false; // Se pelo menos um estiver marcado, não é necessário continuar verificando
        }
    });

    // Retornar verdadeiro se pelo menos um estiver marcado, caso contrário, retornar falso
    return peloMenosUmMarcado;
}