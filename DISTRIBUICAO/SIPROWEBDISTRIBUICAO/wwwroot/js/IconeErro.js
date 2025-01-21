function Addiconeerro(classe, mensagem) {
    // Verifique se já existe um ícone de erro para este campo
    var errorMessageElement = document.querySelector(classe);
    var existingIcon = errorMessageElement.previousElementSibling;

    if (!existingIcon || !existingIcon.classList.contains('error-icon')) {
        // Caso não exista um ícone de erro, crie um novo
        var iconElement = document.createElement('i');
        iconElement.setAttribute('class', 'fa fa-exclamation-triangle error-icon text-warning');

        // Insira o ícone de erro antes do elemento de mensagem de erro
        errorMessageElement.insertAdjacentElement('beforebegin', iconElement);

        // Defina a mensagem de erro
        $(classe).text(mensagem);

        // Adicione uma classe ao campo para indicar que o ícone de erro foi adicionado
        errorMessageElement.classList.add('error-field');
    } else {
        // Se o ícone de erro já existe, apenas atualize a mensagem de erro
        $(classe).text(mensagem);
    }
}

function Removeiconeerro(classe) {
    // Encontre e remova o ícone de erro associado a este campo
    var errorMessageElement = document.querySelector(classe);
    var iconElement = errorMessageElement.previousElementSibling;

    if (iconElement && iconElement.classList.contains('error-icon')) {
        iconElement.parentNode.removeChild(iconElement);
        // Limpe a mensagem de erro
        $(classe).text('');
    }
}

