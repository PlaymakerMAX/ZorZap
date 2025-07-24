pipeline {
    // Définir un agent global est une bonne pratique, même si chaque étape spécifie le sien.
    // 'any' signifie que le coordinateur du pipeline peut s'exécuter sur n'importe quel agent disponible.
    agent any

    stages {
        // Étape 1: Récupération du code source
        stage('Checkout') {
            steps {
                // 'checkout scm' utilise automatiquement la configuration SCM (Git)
                // définie dans l'interface du job Jenkins. C'est la méthode recommandée.
                checkout scm
            }
        }

        // Étape 2: Compilation de l'application
        stage('Build') {
            // Utilise un conteneur Docker avec le SDK.NET 8 comme environnement de build.
            // Jenkins téléchargera cette image si elle n'est pas présente localement.
            agent {
                docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
            }
            steps {
                // Affiche la version du SDK.NET pour le débogage
                sh 'dotnet --version'
                // Restaure les dépendances NuGet du projet
                sh 'dotnet restore ZorZap.sln'
                // Compile la solution en configuration Release, sans restaurer à nouveau les paquets.
                sh 'dotnet build ZorZap.sln --configuration Release --no-restore'
            }
        }

        // Étape 3: Exécution des tests unitaires
        stage('Test') {
            // Réutilise le même environnement que l'étape de build.
            agent {
                docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
            }
            steps {
                // Exécute les tests sans recompiler et génère un rapport de résultats au format trx.
                sh 'dotnet test ZorZap.sln --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"'
            }
            //post {
                // 'always' s'exécute que l'étape ait réussi ou échoué.
              //  always {
                    // Archive les résultats des tests pour qu'ils soient visibles dans l'interface Jenkins.
                    // Cela permet de suivre l'historique des tests et d'analyser les échecs.
                //    junit '**/test-results.trx'
             //   }
            //}
        }

        // Étape 4: Publication des artéfacts
        stage('Publish') {
            agent {
                docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
            }
            steps {
                // Publie l'API dans une configuration optimisée pour la production.
                // Le résultat est placé dans le dossier './app/publish' du workspace Jenkins,
                // qui sera utilisé pour construire l'image Docker finale.
                sh 'dotnet publish./src/ZorZap.Api/ZorZap.Api.csproj -c Release -o./app/publish'
            }
        }

        // Étape 5: Construction de l'image Docker
        stage('Build Docker Image') {
            // Cette étape n'a pas besoin d'un agent spécifique, juste d'un accès au démon Docker.
            agent any
            steps {
                script {
                    // Utilise le plugin Docker Pipeline pour construire l'image.
                    // Le nom de l'image est 'playmakermax/zorzap' et le tag est le numéro de build Jenkins.
                    // Cela garantit que chaque build produit une image unique et traçable.
                    def dockerImage = docker.build("playmakermax/zorzap:${env.BUILD_NUMBER}", ".")
                    
                    // Optionnel: Pousser l'image vers un registre comme Docker Hub
                    // docker.withRegistry('https://index.docker.io/v1/', 'dockerhub-credentials-id') {
                    //     dockerImage.push()
                    // }
                }
            }
        }
    }
}
