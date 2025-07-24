# Partir de l'image officielle Jenkins avec Java 17
FROM jenkins/jenkins:lts-jdk17

# Changer d'utilisateur pour 'root' afin d'installer des paquets
USER root

# Installer les paquets nécessaires et le client Docker
RUN apt-get update && \
    apt-get install -y lsb-release curl gnupg && \
    curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian \
    $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce-cli

# Rendre le socket Docker accessible au groupe 'jenkins'
RUN groupadd -g 999 docker && usermod -aG docker jenkins

# Revenir à l'utilisateur 'jenkins' pour la sécurité
USER jenkins
