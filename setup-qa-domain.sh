#!/bin/bash

# Script para configurar dominio y certificado SSL para QA
# Uso: ./setup-qa-domain.sh

set -e

echo "🚀 Configurando dominio y certificado SSL para QA..."

# Variables
DOMAIN="qa.mytracksnote.com"
EMAIL="admin@mytracksnote.com"

echo "📋 Verificando requisitos..."

# Verificar que Nginx esté instalado
if ! command -v nginx &> /dev/null; then
    echo "📦 Instalando Nginx..."
    apt update
    apt install -y nginx
fi

# Verificar que Certbot esté instalado
if ! command -v certbot &> /dev/null; then
    echo "📦 Instalando Certbot..."
    apt install -y certbot python3-certbot-nginx
fi

echo "🔧 Configurando Nginx..."

# Crear directorio para el sitio
mkdir -p /var/www/$DOMAIN

# Copiar configuración de Nginx
cp nginx-qa.conf /etc/nginx/sites-available/$DOMAIN

# Crear enlace simbólico
ln -sf /etc/nginx/sites-available/$DOMAIN /etc/nginx/sites-enabled/

# Verificar configuración de Nginx
nginx -t

echo "🔒 Obteniendo certificado SSL..."

# Obtener certificado SSL con Let's Encrypt
certbot --nginx -d $DOMAIN --email $EMAIL --agree-tos --non-interactive

echo "🔄 Reiniciando servicios..."

# Reiniciar Nginx
systemctl restart nginx

# Verificar que los contenedores estén corriendo
cd /opt/coachprime-qa
docker-compose -f docker-compose.qa.yml ps

echo "✅ Configuración completada!"
echo "🌐 Tu aplicación QA estará disponible en: https://$DOMAIN"
echo "📧 Certificado SSL válido hasta: $(certbot certificates | grep 'VALID')"

# Mostrar estado de los servicios
echo "📊 Estado de los servicios:"
systemctl status nginx --no-pager -l
docker-compose -f docker-compose.qa.yml ps 