   
   
   Select
        DIS_ID,
        DIS_MOV_ID,
        DIS_DESTINO_USUARIO --as USU_ORIGEM
    into #Processo
    from Protocolo_Distribuicao where Dis_Id =  12


Declare @SetorOrigem int,
		@SetorSolicitante int

    Set @SetorOrigem = (Select top 1 
						       SETSUBUSU_SETSUB_ID 
					      from SetorSubXUsuario 
				         where SETSUBUSU_USUARIO = @MOVPRO_USUARIO_ORIGEM)--49

    Set @SetorSolicitante = (Select top 1 
						        SETSUBUSU_SETSUB_ID 
					       from SetorSubXUsuario inner join #Processo 
							 on (SETSUBUSU_USUARIO = DIS_DESTINO_USUARIO))

    --Vericiar se a responsta está indo para o solicitante
	if(@MOVPRO_SETOR_DESTINO = @SetorSolicitante)
	begin
		Insert Into Movimentacao_Processo
			 Select MOVPRO_PRT_NUMERO, 
					@SetorOrigem,
					'RUBENSCA',
					Getdate(),
					@MOVPRO_PARECER_ORIGEM,
					'Instrucação encaminhada para o solicitante.',
					@MOVPRO_SETOR_DESTINO,
					'INSTRUCAO->ENCAMINHADO',
					null,   
					1
		from #Processo inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID)
		where MOVPRO_INSTRUCAO = 1
		
		Declare @UltimoID int
			Set @UltimoID = SCOPE_IDENTITY();

				 Update PD
		    Set DIS_DESTINO_STATUS = 'INSTRUCAO->ENCAMINDO'
		   From #Processo P INNER JOIN Protocolo_Distribuicao PD ON(P.DIS_MOV_ID = PD.DIS_MOV_ID)



		Insert into Protocolo_Distribuicao
		Select 
			@UltimoID,
			DIS_DESTINO_USUARIO,
			GETDATE(),
			MOVPRO_USUARIO_ORIGEM AS USU_DESTINO,
			0,
			'RECEBIDO',
			0,
			0,
			NULL
		from #Processo inner join Movimentacao_Processo on(DIS_MOV_ID = MOVPRO_ID) 

	end
	else
		Begin
			Update PD
			   Set DIS_DESTINO_STATUS = 'INSTRUCAO->ENCAMINDO'
			From #Processo P INNER JOIN Protocolo_Distribuicao PD ON(P.DIS_MOV_ID = PD.DIS_MOV_ID)

			Insert Into Movimentacao_Processo
					 Select MOVPRO_PRT_NUMERO, 
							@SetorOrigem,
							@MOVPRO_USUARIO_ORIGEM,
							Getdate(),
							@MOVPRO_PARECER_ORIGEM,
							'Instrucação reencaminhada para outro setor.',
							@MOVPRO_SETOR_DESTINO,
							'RECEBIDO',
							null,   
							1
					from #Processo inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID)
				   where MOVPRO_INSTRUCAO = 1
		end


		
		
	