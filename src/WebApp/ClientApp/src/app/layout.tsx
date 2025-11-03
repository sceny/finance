'use client';

import React from 'react';
import {
    FluentProvider,
    webDarkTheme,
    makeStyles,
} from '@fluentui/react-components';
import { Text } from '@fluentui/react-text';
import { Divider } from '@fluentui/react-divider';
import { Card, CardHeader, CardPreview } from '@fluentui/react-card';

const useStyles = makeStyles({
    container: {
        display: 'flex',
        flexWrap: 'wrap',
        justifyContent: 'space-around',
    },
    item: {
        margin: 5,
        minWidth: 300,
        '@media (max-width: 600px)': {
            width: '100%',
        },
    },
});


export default function RootLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <html lang="en">
            <body>
                <FluentProvider theme={webDarkTheme}>{children}</FluentProvider>
            </body>
        </html>
    );
}
