1 - Launch Instance AWS Ubuntu
2 - Clonar Repositório: https://github.com/karinakuroda/RabbitmqCluster.git
3 - Para conectar no instancia: https://docs.aws.amazon.com/console/ec2/instances/connect/putty
4 - Instalar docker no ubuntu- https://store.docker.com/editions/community/docker-ce-server-ubuntu?tab=description
https://docs.docker.com/engine/installation/linux/centos/
5 - Instalar docker-compose:
	 sudo  curl -L "https://github.com/docker/compose/releases/download/1.11.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
	sudo chmod +x /usr/local/bin/docker-compose
	
https://docs.docker.com/compose/install/

6 - Criar uma pasta no servidor:
	sudo mkdir RabbitmqCluster
7 - Colocar permissão na pasta para conseguir copiar os arquivos:
	sudo chown -R ubuntu:ubuntu RabbitMQCluster 
	(chown -R user:group folder_name)
	groups administrator p ver qual grupo esta o usuario administrator

8 - Fazer a copia dos arquivos do git p/ o servidor, baixar WINSCP ou utilizar o comando:
	C:\> pscp -i C:\Keys\my-key-pair.ppk C:\Sample_file.txt user_name@public_dns:/usr/local/Sample_file.txt
9 - P/ Compilar
	 sudo  docker build -t karinakuroda/rabbitmq-cluster .
10 - P/ INiciar as filas:
	sudo docker-compose up -d
	
11 - Criar load balancer p/ management:
	http://docs.aws.amazon.com/elasticloadbalancing/latest/application/create-application-load-balancer.html
OBSERVAÇÕES:
	para ver os logs
	sudo docker-compose logs
	Caso tenha problemas de permissao nos arquivos:
		sudo chmod +x rabbitmq-cluster /usr/local/bin/
		sudo chmod u+x Dockerfile
		sudo chmod u+x pre-entrypoint.sh
		sudo chmod u+x rabbitmq-cluster
		Fazer um build depois de mudar as permissoes
	Verificar se os arquivos estão no formato UNIX (LF)
	git config core.autocrlf false -> para fazer commit sem alterar padrao do linux
	Select File –> Advanced Save Options
Change Line Endings to Unix (LF)
	
	P/ problemas com docker-compose sudo `which docker-compose` up -d
	