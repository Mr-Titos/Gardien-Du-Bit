window.cryptoHelper = {
    createPasswordKey: async function (password, saltBase64) {
        const encoder = new TextEncoder();
        const passwordBytes = encoder.encode(password);

        const salt = Uint8Array.from(atob(saltBase64), c => c.charCodeAt(0));

        const passwordKey = await window.crypto.subtle.importKey(
            "raw",
            passwordBytes,
            { name: "PBKDF2" },
            false,
            ["deriveBits", "deriveKey"]
        );

        const aesKey = await window.crypto.subtle.deriveKey(
            {
                name: "PBKDF2",
                salt: salt,
                iterations: 5000,
                hash: "SHA-256"
            },
            passwordKey,
            { name: "AES-GCM", length: 256 },
            true,
            ["encrypt", "decrypt"]
        );

        const rawKey = await window.crypto.subtle.exportKey("raw", aesKey);

        const rawKeyBase64 = btoa(String.fromCharCode(...new Uint8Array(rawKey)));

        return rawKeyBase64;
    },

    base64ToBytes: function (base64) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes;
    },

    encryptWithPasswordKey: async function (rawKeyBase64, text) {
        const encoder = new TextEncoder();
        const data = encoder.encode(text);

        const rawBytes = Uint8Array.from(atob(rawKeyBase64), c => c.charCodeAt(0));
        const iv = window.crypto.getRandomValues(new Uint8Array(12));

        const key = await window.crypto.subtle.importKey(
            "raw",
            rawBytes.buffer,
            { name: "AES-GCM" },
            false,
            ["encrypt", "decrypt"]
        );

        const cipherTextWithAuthTag = await window.crypto.subtle.encrypt(
            {
                name: "AES-GCM",
                iv: iv
            },
            key,
            data
        );

        const cipherText = cipherTextWithAuthTag.slice(0, cipherTextWithAuthTag.byteLength - 16);
        const authTag = cipherTextWithAuthTag.slice(cipherTextWithAuthTag.byteLength - 16);

        return {

            EnfCipherTextBase64: btoa(String.fromCharCode(...new Uint8Array(cipherText))),
            EnfAuthTagBase64: btoa(String.fromCharCode(...new Uint8Array(authTag))),
            EnfInitialisationVectorBase64: btoa(String.fromCharCode(...new Uint8Array(iv)))
        };
    },

    decryptWithPasswordKey: async function (rawKeyBase64, encryptedData) {
        const { enfCipherTextBase64, enfAuthTagBase64, enfInitialisationVectorBase64 } = encryptedData;

        const cipherText = this.base64ToBytes(enfCipherTextBase64);
        const iv = this.base64ToBytes(enfInitialisationVectorBase64);
        const authTag = this.base64ToBytes(enfAuthTagBase64);

        const combinedCipherText = new Uint8Array([...cipherText, ...authTag]);
        const decoder = new TextDecoder();

        const rawBytes = Uint8Array.from(atob(rawKeyBase64), c => c.charCodeAt(0));

        const key = await window.crypto.subtle.importKey(
            "raw",
            rawBytes.buffer,
            { name: "AES-GCM" },
            false,
            ["encrypt", "decrypt"]
        );

        const decryptedData = await window.crypto.subtle.decrypt(
            {
                name: "AES-GCM",
                iv: new Uint8Array(iv),
            },
            key,
            combinedCipherText
        );

        return decoder.decode(decryptedData);
    },


    encryptRSAOAEP: async function (publicKeyBase64, dataUtf8) {
        const binaryDer = Uint8Array.from(atob(publicKeyBase64), c => c.charCodeAt(0));

        const key = await window.crypto.subtle.importKey(
            "spki",
            binaryDer.buffer,
            {
                name: "RSA-OAEP",
                hash: "SHA-256"
            },
            false,
            ["encrypt"]
        );

        const encoder = new TextEncoder();
        const encrypted = await window.crypto.subtle.encrypt(
            {
                name: "RSA-OAEP"
            },
            key,
            encoder.encode(dataUtf8)
        );

        return btoa(String.fromCharCode(...new Uint8Array(encrypted)));
    },

    decryptRSAOAEP: async function (privateKeyBase64, encryptedBase64) {
        try {
            const privateKeyBytes = Uint8Array.from(atob(privateKeyBase64), c => c.charCodeAt(0));
            const encryptedBytes = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));

            const privateKey = await window.crypto.subtle.importKey(
                "pkcs8",
                privateKeyBytes.buffer,
                {
                    name: "RSA-OAEP",
                    hash: "SHA-256"
                },
                false,
                ["decrypt"]
            );

            const decryptedBuffer = await window.crypto.subtle.decrypt(
                {
                    name: "RSA-OAEP"
                },
                privateKey,
                encryptedBytes
            );

            const decoder = new TextDecoder();
            return decoder.decode(decryptedBuffer);
        } catch (e) {
            console.error("Decryption failed", e);
            throw e;
        }
    }
};