// Função para renovar o token
function refreshToken() {
    // Obter o token atual armazenado (exemplo usando localStorage, mas pode ser usado sessionStorage ou cookies)
    const currentToken = localStorage.getItem('Token');

    if (!currentToken) {
        console.log('Token não encontrado!');
        return;
    }

    fetch('/sipro/autenticacao/refresh', {
        method: 'POST',  // Pode ser GET ou POST, dependendo do seu back-end
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${currentToken}`  // Enviar o token atual no cabeçalho Authorization
        },
        credentials: 'include',  // Para incluir cookies ou sessões, se necessário
    })
        .then(response => {
            if (response.ok) {
                return response.json();  // Recebe o novo token da resposta da API
            } else {
                throw new Error('Falha ao renovar o token');
            }
        })
        .then(data => {
            // Atualiza o token armazenado no cliente
            localStorage.setItem('Token', data.token);  // Atualiza o token no localStorage
            console.log('Token renovado com sucesso!');
        })
        .catch(error => {
            console.error('Erro ao renovar o token:', error);
        });
}

// Configurar o intervalo para chamar o refresh a cada 15 minutos
setInterval(refreshToken, 15 * 60 * 1000);  // A cada 15 minutos
