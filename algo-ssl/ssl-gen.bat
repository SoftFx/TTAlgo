@echo off

set SSL_DIR=.
set OPEN_SSL=%SSL_DIR%\openssl.exe
set SSL_CNF=%SSL_DIR%\openssl.cnf

set BASE_DIR=.
set KEYS_DIR=keys
set CRTS_DIR=crts
set REQS_DIR=reqs
REM mkdir %BASE_DIR%
mkdir %KEYS_DIR%
mkdir %CRTS_DIR%
mkdir %REQS_DIR%

set CA_KEY=%BASE_DIR%/%KEYS_DIR%/algo-ca.key
set CA_CRT=%BASE_DIR%/%CRTS_DIR%/algo-ca.crt
set CA_PFX=%BASE_DIR%/%CRTS_DIR%/algo-ca.pfx
set BA_KEY=%BASE_DIR%/%KEYS_DIR%/bot-agent.key
set BA_REQ=%BASE_DIR%/%REQS_DIR%/bot-agent.csr
set BA_CRT=%BASE_DIR%/%CRTS_DIR%/bot-agent.crt
set BA_PFX=%BASE_DIR%/%CRTS_DIR%/bot-agent.pfx
set BT_KEY=%BASE_DIR%/%KEYS_DIR%/bot-terminal.key
set BT_REQ=%BASE_DIR%/%REQS_DIR%/bot-terminal.csr
set BT_CRT=%BASE_DIR%/%CRTS_DIR%/bot-terminal.crt
set BT_PFX=%BASE_DIR%/%CRTS_DIR%/bot-terminal.pfx
set BT_SIGN_KEY=%BASE_DIR%/%KEYS_DIR%/bot-terminal-sign.key
set BT_SIGN_CRT=%BASE_DIR%/%CRTS_DIR%/bot-terminal-sign.crt
set BT_SIGN_PFX=%BASE_DIR%/%CRTS_DIR%/bot-terminal-sign.pfx

%OPEN_SSL% genrsa -out %CA_KEY% 4096
%OPEN_SSL% req -x509 -sha512 -days 36500 -config %SSL_CNF% -key %CA_KEY% -out %CA_CRT% -subj "/C=LV/L=Riga/O=softFX/OU=TTAlgo/CN=tt-algo.soft-fx.lv"
%OPEN_SSL% pkcs12 -export -in %CA_CRT% -inkey %CA_KEY% -out %CA_PFX% -passout pass:""

%OPEN_SSL% genrsa -out %BA_KEY% 4096
%OPEN_SSL% req -new -sha512 -config %SSL_CNF% -extensions server_cert -key %BA_KEY% -out %BA_REQ% -subj "/C=LV/L=Riga/O=softFX/OU=BotAgent/CN=bot-agent.soft-fx.lv"
%OPEN_SSL% x509 -req -sha512 -days 36500 -extfile %SSL_CNF% -extensions server_cert -CA %CA_CRT% -CAkey %CA_KEY% -set_serial 1 -in %BA_REQ% -out %BA_CRT%
%OPEN_SSL% pkcs12 -export -in %BA_CRT% -inkey %BA_KEY% -out %BA_PFX% -passout pass:"" -CAfile %CA_CRT% -chain

%OPEN_SSL% genrsa -out %BT_KEY% 4096
%OPEN_SSL% req -new -sha512 -config %SSL_CNF% -extensions client_cert -key %BT_KEY% -out %BT_REQ% -subj "/C=LV/L=Riga/O=softFX/OU=BotTerminal"
%OPEN_SSL% x509 -req -sha512 -days 36500 -extfile %SSL_CNF% -extensions client_cert -CA %CA_CRT% -CAkey %CA_KEY% -set_serial 2 -in %BT_REQ% -out %BT_CRT%
%OPEN_SSL% pkcs12 -export -in %BT_CRT% -inkey %BT_KEY% -out %BT_PFX% -passout pass:"" -CAfile %CA_CRT% -chain

%OPEN_SSL% genrsa -out %BT_SIGN_KEY% 4096
%OPEN_SSL% req -x509 -sha512 -days 36500 -config %SSL_CNF% -extensions client_sign_cert -key %BT_SIGN_KEY% -out %BT_SIGN_CRT% -subj "/C=LV/L=Riga/O=softFX/OU=TTAlgo/CN=bot-terminal.soft-fx.lv"
%OPEN_SSL% pkcs12 -export -in %BT_SIGN_CRT% -inkey %BT_SIGN_KEY% -out %BT_SIGN_PFX% -passout pass:""

pause