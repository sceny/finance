'use client';

import {
    Button,
    makeStyles,
    shorthands,
    Title1,
    tokens,
} from '@fluentui/react-components';
import type { NextPage } from 'next';
import Head from 'next/head';
import { Text } from '@fluentui/react-text';
import { Card, CardHeader, CardPreview } from '@fluentui/react-card';

const useStyles = makeStyles({
    container: {
        display: 'flex',
        flexDirection: 'column',
        width: '200px',

        ...shorthands.border('2px', 'dashed', tokens.colorPaletteBerryBorder2),
        ...shorthands.borderRadius(tokens.borderRadiusMedium),
        ...shorthands.gap('5px'),
        ...shorthands.padding('10px'),
    },
});

const Home: NextPage = () => {
    const styles = useStyles();

    return (
        <>
            <Head>
                <title>My app</title>
            </Head>

            <div className={styles.container}>
                <div className={styles.item}>
                    <Card>
                        <CardHeader>
                            <Text >Card 1</Text>
                        </CardHeader>
                        <CardPreview>
                            <Text>
                                Fluent UI is a collection of utilities, React
                                components, and tokens for building web
                                applications.
                            </Text>
                        </CardPreview>
                    </Card>
                </div>
                <div className={styles.item}>
                    <Card>
                        <CardHeader>
                            <Text variant="large">Card 2</Text>
                        </CardHeader>
                        <CardPreview>
                            <Text>
                                Fluent UI provides you with a robust and
                                flexible toolset to create seamless experiences
                                in less time.
                            </Text>
                        </CardPreview>
                    </Card>
                </div>
                <div className={styles.item}>
                    <Card>
                        <CardHeader>
                            <Text variant="large">Card 3</Text>
                        </CardHeader>
                        <CardPreview>
                            <Text>
                                Fluent UI includes a full suite of SharePoint
                                web parts, Office Add-ins, Teams apps, and more.
                            </Text>
                        </CardPreview>
                    </Card>
                </div>
            </div>

            <div className={styles.container}>
                <Title1>Hello world!</Title1>
                <Button onClick={() => alert('ok')}>A button</Button>
            </div>
        </>
    );
};

export default Home;
