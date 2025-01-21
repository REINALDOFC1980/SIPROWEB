//function formatValueWithLeadingZeros(inputValue) {
//    var letter = inputValue.match(/[a-zA-Z]+/)[0].toUpperCase(); // Extrai a letra do valor
//    var number = inputValue.match(/\d+/)[0]; // Extrai os números do valor
//    var paddedNumber = number.padStart(9, '0'); // Completa com zeros à esquerda até atingir 10 caracteres
//    return letter + paddedNumber; // Retorna o valor formatado
//}

function formatValueWithLeadingZeros(inputValue) {
    // Extrai a letra do valor e converte para maiúsculas
    var letterMatch = inputValue.match(/[a-zA-Z]+/);
    var letter = letterMatch ? letterMatch[0].toUpperCase() : ''; // Se não encontrar, retorna uma string vazia
    // Extrai os números do valor e completa com zeros à esquerda até atingir 9 caracteres
    var numberMatch = inputValue.match(/\d+/);
    var paddedNumber = numberMatch ? numberMatch[0].padStart(9, '0') : ''; // Se não encontrar, retorna uma string vazia
    // Retorna o valor formatado
    return letter + paddedNumber;
}