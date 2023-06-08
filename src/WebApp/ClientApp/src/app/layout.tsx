'use client';

import { FluentProvider, teamsLightTheme } from '@fluentui/react-components';

export default function RootLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return (
        <html lang="en">
            <body>
                <FluentProvider theme={teamsLightTheme}>
                    {children}
                </FluentProvider>
            </body>
        </html>
    );
}
