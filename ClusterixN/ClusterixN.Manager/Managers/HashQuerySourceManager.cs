#region Copyright
/*
 * Copyright 2017 Roman Klassen
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

﻿using System.Collections.Generic;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Enum;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Infrastructure.Base;
using ClusterixN.Common.Utils;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantEmptyObjectOrCollectionInitializer

namespace ClusterixN.Manager.Managers
{
    /// <summary>
    ///     Создает запросы на выполнение из теста TPC-H
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class HashQuerySourceManager : QuerySourceManagerBase
    {
        #region Queries

        /// <summary>
        ///     Запрос № 1 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q1()
        {
            var builder = new QueryBuilder(1);

            #region SELECT

            var select1 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	L_RETURNFLAG,
	L_LINESTATUS,
	L_QUANTITY,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_TAX
FROM
	LINEITEM
WHERE
	L_SHIPDATE <= DATE '1998-12-01' - INTERVAL '90' DAY;", 1));

            #endregion

            #region JOIN



            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	L_RETURNFLAG,
	L_LINESTATUS,
	SUM(L_QUANTITY) AS SUM_QTY,
	SUM(L_EXTENDEDPRICE) AS SUM_BASE_PRICE,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS SUM_DISC_PRICE,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT) * (1 + L_TAX)) AS SUM_CHARGE,
	AVG(L_QUANTITY) AS AVG_QTY,
	AVG(L_EXTENDEDPRICE) AS AVG_PRICE,
	AVG(L_DISCOUNT) AS AVG_DISC,
	COUNT(*) AS COUNT_ORDER
FROM
	" + Constants.RelationNameTag + @"
GROUP BY
	L_RETURNFLAG,
	L_LINESTATUS
ORDER BY
	L_RETURNFLAG,
	L_LINESTATUS;",
                builder.CreateRelationSchema(new List<Field>()
                {
                    new Field() {Name = "L_RETURNFLAG", Params = "CHAR(1) NOT NULL"},
                    new Field() {Name = "L_LINESTATUS", Params = "CHAR(1) NOT NULL"},
                    new Field() {Name = "SUM_QTY", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "SUM_BASE_PRICE", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "SUM_DISC_PRICE", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "SUM_CHARGE", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "AVG_QTY", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "AVG_PRICE", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "AVG_DISC", Params = "DECIMAL(18,2) NULL DEFAULT NULL"},
                    new Field() {Name = "COUNT_ORDER", Params = "BIGINT NOT NULL DEFAULT '0'"},
                }, new List<Index>()),
                1,
                @"SELECT
	L_RETURNFLAG,
	L_LINESTATUS,
	SUM_QTY,
	SUM_BASE_PRICE,
	SUM_DISC_PRICE,
	SUM_CHARGE,
	AVG_QTY,
	AVG_PRICE,
	AVG_DISC,
	COUNT_ORDER
FROM
	" + Constants.RelationNameTag + @"",
                builder.CreateRelation(select1, "sort",
                    builder.CreateRelationSchema(new List<Field>()
                    {
                        new Field() {Name = "L_RETURNFLAG", Params = "CHAR(1) NOT NULL"},
                        new Field() {Name = "L_LINESTATUS", Params = "CHAR(1) NOT NULL"},
                        new Field() {Name = "L_QUANTITY", Params = "DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = "DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = "DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_TAX", Params = "DECIMAL(15,2) NOT NULL"},
                    }, new List<Index>())
)));

            #endregion

            return builder.GetQuery();
        }
        
        /// <summary>
        ///     Запрос № 2 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q2()
        {
            var builder = new QueryBuilder(2);

            #region SELECT

            var selectPS2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	PS_PARTKEY,
	PS_SUPPKEY,
	PS_SUPPLYCOST
FROM
	PARTSUPP", 1));

            var selectS2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	S_SUPPKEY,
	S_NATIONKEY
FROM
	SUPPLIER", 2));

            var selectN2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NATIONKEY,
	N_REGIONKEY
FROM
	NATION", 3));

            var selectR2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	R_REGIONKEY
FROM
	REGION
WHERE
	R_NAME = 'AMERICA'", 4));

            var selectP = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	P_PARTKEY,
	P_MFGR
FROM
	PART
WHERE
	P_SIZE = 48
	AND P_TYPE LIKE '%NICKEL'", 5));

            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
    S_ACCTBAL,
	S_NAME,
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_SUPPKEY,
	S_NATIONKEY
FROM
	SUPPLIER", 6));

            var selectPS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	PS_PARTKEY,
	PS_SUPPKEY,
	PS_SUPPLYCOST
FROM
	PARTSUPP", 7));

            var selectN = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NAME,
	N_REGIONKEY,
	N_NATIONKEY
FROM
	NATION", 8));

            var selectR = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	R_REGIONKEY
FROM
	REGION
WHERE 
	R_NAME = 'AMERICA'", 9));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	N_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS N2,
                                    " + Constants.RightRelationNameTag + @" AS R2
                        WHERE
                        N_REGIONKEY = R_REGIONKEY",
                        builder.CreateRelationSchema(
                            new List<Field>()
                            {
                                new Field() {Name = "N_NATIONKEY", Params = "INT NOT NULL"} 
                                
                            }, 
                            new List<Index>()
                            {
                                new Index() {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY" }},
                            }),
                1,
                builder.CreateRelation(selectN2, "N2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NATIONKEY", Params = "INT NOT NULL"},
                            new Field() {Name = "N_REGIONKEY", Params = "INT NOT NULL"},

                        },
                        new List<Index>()
                        {
                            new Index() {Name = "N_REGIONKEY", FieldNames = new List<string>() { "N_REGIONKEY" }},
                        })),
                builder.CreateRelation(selectR2, "R2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "R_REGIONKEY", Params = "INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "R_REGIONKEY", FieldNames = new List<string>() { "R_REGIONKEY" }},
                        })
)));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_SUPPKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J01,
                                    " + Constants.RightRelationNameTag + @" AS S2
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                    }),
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectS2, "S2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                        })
                        )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	PS_PARTKEY,
	PS_SUPPLYCOST
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J02,
                                    " + Constants.RightRelationNameTag + @" AS PS2
                        WHERE
                        S_SUPPKEY = PS_SUPPKEY", 
    builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "PS_PARTKEY", FieldNames = new List<string>() { "PS_PARTKEY"} },
                    })
                ,
                3,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectPS2, "PS2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "PS_SUPPKEY", FieldNames = new List<string>() { "PS_SUPPKEY"} },
                        })
                        ),
                @"SELECT
	PS_PARTKEY,
	PS_SUPPLYCOST
FROM
    " + Constants.RelationNameTag + @""));

            var join11 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_ACCTBAL,
	S_NAME,
	S_NATIONKEY,
	PS_SUPPLYCOST,
	PS_PARTKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS PS,
                                    " + Constants.RightRelationNameTag + @" AS S
                        WHERE
                        S_SUPPKEY = PS_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                        new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "PS_PARTKEY", FieldNames = new List<string>() { "PS_PARTKEY"} },
                    }),
                5,
                builder.CreateRelation(selectPS, "PS",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "PS_SUPPKEY", FieldNames = new List<string>() { "PS_SUPPKEY" } },
                        })
                ),
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                            new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                            new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                            new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                        })
                )));

            var join12 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_ACCTBAL,
	S_NAME,
	P_PARTKEY,
	P_MFGR,
	S_NATIONKEY,
	PS_SUPPLYCOST
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J11,
                                    " + Constants.RightRelationNameTag + @" AS P
                        WHERE
                        P_PARTKEY = PS_PARTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                        new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "P_MFGR", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    }),
                4,
                builder.CreateRelation(join11),
                builder.CreateRelation(selectP, "P",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "P_MFGR", Params = " CHAR(25) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "P_PARTKEY", FieldNames = new List<string>() { "P_PARTKEY"} },
                        }))));

            var join13 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_ACCTBAL,
	S_NAME,
	P_PARTKEY,
	P_MFGR,
	PS_SUPPLYCOST,
	N_REGIONKEY,
	N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J12,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                        new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "P_MFGR", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_REGIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "N_REGIONKEY", FieldNames = new List<string>() { "N_REGIONKEY"} },
                    }),
                4,
                builder.CreateRelation(join12),
                builder.CreateRelation(selectN, "N",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                            new Field() {Name = "N_REGIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                )));

            var join14 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_ACCTBAL,
	S_NAME,
	P_PARTKEY,
	P_MFGR,
	PS_SUPPLYCOST,
	N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J13,
                                    " + Constants.RightRelationNameTag + @" AS R
                        WHERE
                        N_REGIONKEY = R_REGIONKEY", 
                        builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                        new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "P_MFGR", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "P_PARTKEY", FieldNames = new List<string>() { "P_PARTKEY"} },
                    })
                ,
                4,
                builder.CreateRelation(join13),
                builder.CreateRelation(selectR, "R",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "R_REGIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "R_REGIONKEY", FieldNames = new List<string>() { "R_REGIONKEY"} },
                        })
                ),
                @"SELECT
	S_ADDRESS,
	S_PHONE,
	S_COMMENT,
	S_ACCTBAL,
	S_NAME,
	P_PARTKEY,
	P_MFGR,
	PS_SUPPLYCOST,
	N_NAME
FROM
    " + Constants.RelationNameTag + @""
                ));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	S_ACCTBAL,
	S_NAME,
	N_NAME,
	P_PARTKEY,
	P_MFGR,
	S_ADDRESS,
	S_PHONE,
	S_COMMENT
FROM
	" + Constants.RelationNameTag + @" AS J14
WHERE
    PS_SUPPLYCOST = 
	(
		SELECT
			MIN(PS_SUPPLYCOST)
		FROM
			" + Constants.RelationNameTag + @" AS PM
		WHERE
			P_PARTKEY = PM.PS_PARTKEY
	)
ORDER BY
	S_ACCTBAL DESC,
	N_NAME,
	S_NAME,
	P_PARTKEY;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "P_MFGR", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "S_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "S_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_COMMENT", Params = " VARCHAR(101) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    }),
                1,
                @"SELECT
	S_ACCTBAL,
	S_NAME,
	N_NAME,
	P_PARTKEY,
	P_MFGR,
	S_ADDRESS,
	S_PHONE,
	S_COMMENT
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join14, QueryRelationStatus.Wait),
                builder.CreateRelation(join3, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 3 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q3()
        {
            var builder = new QueryBuilder(3);

            #region SELECT

            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	C_CUSTKEY
FROM
	CUSTOMER
WHERE 
    C_MKTSEGMENT = 'HOUSEHOLD'", 1));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_ORDERDATE,
	O_SHIPPRIORITY,
	O_ORDERKEY,
	O_CUSTKEY
FROM
	ORDERS
WHERE 
    O_ORDERDATE < DATE '1995-03-31';", 2));

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_ORDERKEY,
	L_EXTENDEDPRICE,
	L_DISCOUNT
FROM
	LINEITEM
WHERE
	L_SHIPDATE > DATE '1995-03-31'", 3));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	O_ORDERDATE,
	O_SHIPPRIORITY,
	O_ORDERKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS C,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        C_CUSTKEY = O_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "O_SHIPPRIORITY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectC, "C",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                        })
                ),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                            new Field() {Name = "O_SHIPPRIORITY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                        }))));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	O_ORDERDATE,
	O_SHIPPRIORITY,
	L_ORDERKEY,
	L_EXTENDEDPRICE,
	L_DISCOUNT
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J01,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        L_ORDERKEY = O_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "O_SHIPPRIORITY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                        })
                ),
                @"SELECT
	O_ORDERDATE,
	O_SHIPPRIORITY,
	L_ORDERKEY,
	L_EXTENDEDPRICE,
	L_DISCOUNT
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	L_ORDERKEY,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,
	O_ORDERDATE,
	O_SHIPPRIORITY
FROM
	" + Constants.RelationNameTag + @" AS J01
GROUP BY
	L_ORDERKEY,
	O_ORDERDATE,
	O_SHIPPRIORITY
ORDER BY
	REVENUE DESC,
	O_ORDERDATE;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "REVENUE", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "O_SHIPPRIORITY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                    }),
                1,
                @"SELECT
	L_ORDERKEY,
	REVENUE,
	O_ORDERDATE,
	O_SHIPPRIORITY
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join2, QueryRelationStatus.Wait)));
            
            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 4 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q4()
        {
            var builder = new QueryBuilder(4);

            #region SELECT

            var select1 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT DISTINCT
	O_ORDERPRIORITY,
	O_ORDERKEY
FROM
	ORDERS
WHERE 
    O_ORDERDATE >= DATE '1996-02-01'
    AND O_ORDERDATE < DATE '1996-02-01' + INTERVAL '3' MONTH;", 1));
            var select2 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT DISTINCT
	L_ORDERKEY
FROM
	LINEITEM
WHERE
	L_COMMITDATE < L_RECEIPTDATE;", 2));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
                                O_ORDERPRIORITY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS O,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        L_ORDERKEY = O_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    }),
                1,
                builder.CreateRelation(select1, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                        })
                ),
                builder.CreateRelation(select2, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                        })
                ),
                @"SELECT
    O_ORDERPRIORITY
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
    O_ORDERPRIORITY,
    COUNT(*) AS ORDER_COUNT
FROM
	" + Constants.RelationNameTag + @"
GROUP BY
    O_ORDERPRIORITY
ORDER BY
    O_ORDERPRIORITY;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "ORDER_COUNT", Params = " BIGINT NOT NULL DEFAULT '0'"},
                    },
                    new List<Index>()
                    {
                    }),
                1,
                @"SELECT
    O_ORDERPRIORITY,
    ORDER_COUNT
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join1, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 5 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q5()
        {
            var builder = new QueryBuilder(5);

            #region SELECT

            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	C_CUSTKEY,
	C_NATIONKEY
FROM
	CUSTOMER", 1));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_CUSTKEY,
	O_ORDERKEY
FROM
	ORDERS
WHERE 
	O_ORDERDATE >= DATE '1995-01-01'
	AND O_ORDERDATE < DATE '1995-01-01' + INTERVAL '1' YEAR", 2));

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_SUPPKEY
FROM
	LINEITEM", 3));

            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	S_NATIONKEY,
	S_SUPPKEY
FROM
	SUPPLIER", 4));

            var selectN = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NAME,
	N_NATIONKEY,
	N_REGIONKEY
FROM
	NATION", 5));

            var selectR = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	R_REGIONKEY
FROM
	REGION
WHERE
	R_NAME = 'MIDDLE EAST'", 6));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	C_NATIONKEY,
	O_ORDERKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS C,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        C_CUSTKEY = O_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectC, "C",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                        })
                ),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_SUPPKEY,
	C_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J01,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        L_ORDERKEY = O_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_SUPPKEY", FieldNames = new List<string>() { "L_SUPPKEY"} },
                        new Index() {Name = "C_NATIONKEY", FieldNames = new List<string>() { "C_NATIONKEY"} },
                    }),
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                        })
                )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J02,
                                    " + Constants.RightRelationNameTag + @" AS S
                        WHERE
                        L_SUPPKEY = S_SUPPKEY AND C_NATIONKEY = S_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY", "S_SUPPKEY" } },
                        }))));

            var join4 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N_REGIONKEY,
	N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J03,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_REGIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "N_REGIONKEY", FieldNames = new List<string>() { "N_REGIONKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join3),
                builder.CreateRelation(selectN, "N",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "N_REGIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                )));

            var join5 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J04,
                                    " + Constants.RightRelationNameTag + @" AS R
                        WHERE
                        N_REGIONKEY = R_REGIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                2,
                builder.CreateRelation(join4),
                builder.CreateRelation(selectR, "R",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "R_REGIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "R_REGIONKEY", FieldNames = new List<string>() { "R_REGIONKEY"} },
                        })
                ),
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N_NAME
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	N_NAME,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE
FROM
	" + Constants.RelationNameTag + @" AS J05
GROUP BY
	N_NAME
ORDER BY
	REVENUE DESC;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "REVENUE", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	N_NAME,
	REVENUE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join5, QueryRelationStatus.Wait)));

            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 6 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q6()
        {
            var builder = new QueryBuilder(6);

            #region SELECT

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT
FROM
	LINEITEM
WHERE
	L_SHIPDATE >= DATE '1997-01-01'
	AND L_SHIPDATE < DATE '1997-01-01' + INTERVAL '1' YEAR
	AND L_DISCOUNT BETWEEN 0.07 - 0.01 AND 0.07 + 0.01
	AND L_QUANTITY < 24", 1));


            #endregion

            #region JOIN



            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	SUM(L_EXTENDEDPRICE * L_DISCOUNT) AS REVENUE
FROM
	" + Constants.RelationNameTag + @" AS L
",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "REVENUE", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	REVENUE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        },
                        new List<Index>()
                        {
                        })
                )));

            #endregion


            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 7 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q7()
        {
            var builder = new QueryBuilder(7);

            #region SELECT

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_SHIPDATE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_SUPPKEY,
	L_EXTENDEDPRICE
FROM
	LINEITEM
WHERE 
	L_SHIPDATE BETWEEN DATE '1995-01-01' AND DATE '1996-12-31'", 1));

            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	S_SUPPKEY,
	S_NATIONKEY
FROM
	SUPPLIER", 2));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_ORDERKEY,
	O_CUSTKEY
FROM
	ORDERS", 3));

            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	C_NATIONKEY,
	C_CUSTKEY
FROM
	CUSTOMER", 4));

            var selectN1 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NATIONKEY,
	N_NAME
FROM
	NATION
WHERE
	N_NAME = 'IRAQ' OR N_NAME = 'ALGERIA'", 5));

            var selectN2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NATIONKEY,
	N_NAME
FROM
	NATION
WHERE
	N_NAME = 'IRAQ' OR N_NAME = 'ALGERIA'", 6));
            
            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS S,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        S_SUPPKEY = L_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                    })
            ,
                1,
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                        })
                ),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_SUPPKEY", FieldNames = new List<string>() { "L_SUPPKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	S_NATIONKEY,
	O_CUSTKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J01,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        O_ORDERKEY = L_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                        })
                )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	O_CUSTKEY,
	N_NAME AS N1_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J02,
                                    " + Constants.RightRelationNameTag + @" AS N1
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "N1_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectN1, "N1",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N1_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                )));

            var join4 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
    N1_NAME,
	C_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J03,
                                    " + Constants.RightRelationNameTag + @" AS C
                        WHERE
                        C_CUSTKEY = O_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N1_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "C_NATIONKEY", FieldNames = new List<string>() { "C_NATIONKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join3),
                builder.CreateRelation(selectC, "C", 
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                    }))));

            var join5 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N1_NAME AS SUPP_NATION,
	N_NAME AS CUST_NATION
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J04,
                                    " + Constants.RightRelationNameTag + @" AS N2
                        WHERE
                        C_NATIONKEY = N_NATIONKEY and N1_NAME <> N_NAME",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "SUPP_NATION", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "CUST_NATION", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                2,
                builder.CreateRelation(join4),
                builder.CreateRelation(selectN2, "N2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N2_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                ),
                @"SELECT
	L_SHIPDATE,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	SUPP_NATION,
	CUST_NATION
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	SUPP_NATION,
	CUST_NATION,
	EXTRACT(YEAR FROM L_SHIPDATE) AS L_YEAR,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE
FROM
	" + Constants.RelationNameTag + @" AS J5
GROUP BY
	SUPP_NATION,
	CUST_NATION,
	L_YEAR
ORDER BY
	SUPP_NATION,
	CUST_NATION,
	L_YEAR;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "SUPP_NATION", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "CUST_NATION", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "L_YEAR", Params = " INT(4) NULL DEFAULT NULL"},
                        new Field() {Name = "REVENUE", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	SUPP_NATION,
	CUST_NATION,
	L_YEAR,
	REVENUE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join5, QueryRelationStatus.Wait)));

            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 8 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q8()
        {
            var builder = new QueryBuilder(8);

            #region SELECT

            var selectP = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	P_PARTKEY
FROM
	PART
WHERE
	P_TYPE = 'STANDARD BRUSHED BRASS'", 1));

            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	S_SUPPKEY,
	S_NATIONKEY
FROM
	SUPPLIER", 2));

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_SUPPKEY,
	L_PARTKEY
FROM
	LINEITEM", 3));

            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	C_NATIONKEY,
	C_CUSTKEY
FROM
	CUSTOMER", 4));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_ORDERDATE,
	O_ORDERKEY,
	O_CUSTKEY
FROM
	ORDERS
WHERE
	O_ORDERDATE BETWEEN DATE '1995-01-01' AND DATE '1996-12-31'", 2));

            var selectN1 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_REGIONKEY,
	N_NATIONKEY
FROM
	NATION", 5));

            var selectN2 = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NATIONKEY,
	N_NAME
FROM
	NATION", 6));

            var selectR = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	R_REGIONKEY
FROM
	REGION
WHERE
	R_NAME = 'MIDDLE EAST'", 7));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_SUPPKEY,
	L_ORDERKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS P,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        P_PARTKEY = L_PARTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_SUPPKEY", FieldNames = new List<string>() { "L_SUPPKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectP, "P",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "P_PARTKEY", FieldNames = new List<string>() { "P_PARTKEY"} },
                        })
                ),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_PARTKEY", FieldNames = new List<string>() { "L_PARTKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J1,
                                    " + Constants.RightRelationNameTag + @" AS S
                        WHERE
                        S_SUPPKEY = L_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                        })
                )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	S_NATIONKEY,
	O_CUSTKEY,
	O_ORDERDATE
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J2,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        L_ORDERKEY = O_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                    })
                ,
                3,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                        })
                )));

            var join4 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	S_NATIONKEY,
	C_NATIONKEY,
	O_ORDERDATE
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J3,
                                    " + Constants.RightRelationNameTag + @" AS C
                        WHERE
                        O_CUSTKEY = C_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "C_NATIONKEY", FieldNames = new List<string>() { "C_NATIONKEY"} },
                    })
                ,
                3,
                builder.CreateRelation(join3),
                builder.CreateRelation(selectC, "C",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                        })
                )));

            var join5 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	S_NATIONKEY,
	N1.N_REGIONKEY AS N1_N_REGIONKEY,
	O_ORDERDATE
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J4,
                                    " + Constants.RightRelationNameTag + @" AS N1
                        WHERE
                        C_NATIONKEY = N1.N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "N1_N_REGIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                3,
                builder.CreateRelation(join4),
                builder.CreateRelation(selectN1, "N1",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_REGIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                )));

            var join6 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N1_N_REGIONKEY,
	O_ORDERDATE,
	N2.N_NAME AS N2_N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J5,
                                    " + Constants.RightRelationNameTag + @" AS N2
                        WHERE
                        S_NATIONKEY = N2.N_NATIONKEY", 
                        builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N1_N_REGIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "N2_N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "N1_N_REGIONKEY", FieldNames = new List<string>() { "N1_N_REGIONKEY"} },
                    })
                ,
                3,
                builder.CreateRelation(join5),
                builder.CreateRelation(selectN2, "N2", 
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                    })
                )));

            var join7 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	EXTRACT(YEAR FROM O_ORDERDATE) AS O_YEAR,
	L_EXTENDEDPRICE * (1 - L_DISCOUNT) AS VOLUME,
	N2_N_NAME AS NATION
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J6,
                                    " + Constants.RightRelationNameTag + @" AS R
                        WHERE
                        N1_N_REGIONKEY = R_REGIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_YEAR", Params = " INT(4) NULL DEFAULT NULL"},
                        new Field() {Name = "VOLUME", Params = " DECIMAL(18,4) NOT NULL DEFAULT '0.0000'"},
                        new Field() {Name = "NATION", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                3,
                builder.CreateRelation(join6),
                builder.CreateRelation(selectR, "R",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "R_REGIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "R_REGIONKEY", FieldNames = new List<string>() { "R_REGIONKEY"} },
                    })
                ),
                @"SELECT
	O_YEAR,
	VOLUME,
	NATION
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	O_YEAR,
	SUM(CASE
		WHEN NATION = 'IRAN' THEN VOLUME
		ELSE 0
	END) / SUM(VOLUME) AS MKT_SHARE
FROM
	" + Constants.RelationNameTag + @" AS J7
GROUP BY
	O_YEAR
ORDER BY
	O_YEAR;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "O_YEAR", Params = " INT(4) NULL DEFAULT NULL"},
                        new Field() {Name = "MKT_SHARE", Params = " DECIMAL(18,8) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	O_YEAR,
	MKT_SHARE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join7, QueryRelationStatus.Wait)));

            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 9 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q9()
        {
            var builder = new QueryBuilder(9);

            #region SELECT

            var selectP = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	P_PARTKEY
FROM
	PART
WHERE
	P_NAME LIKE '%SNOW%'", 1));

            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	S_SUPPKEY,
	S_NATIONKEY
FROM
	SUPPLIER", 2));

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_QUANTITY,
	L_SUPPKEY,
	L_PARTKEY
FROM
	LINEITEM", 3));

            var selectPS = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	PS_SUPPLYCOST,
	PS_SUPPKEY,
	PS_PARTKEY
FROM
	PARTSUPP", 4));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_ORDERDATE,
	O_ORDERKEY,
	O_ORDERPRIORITY
FROM
	ORDERS", 5));

            var selectN = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NAME,
	N_NATIONKEY
FROM
	NATION", 5));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_QUANTITY,
	L_SUPPKEY,
	L_PARTKEY,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS S,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        S_SUPPKEY = L_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_QUANTITY", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_PARTKEY_L_SUPPKEY", FieldNames = new List<string>() { "L_PARTKEY" , "L_SUPPKEY" } },
                    })
                ,
                1,
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                        })
                ),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_QUANTITY", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_SUPPKEY", FieldNames = new List<string>() { "L_SUPPKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_QUANTITY,
	L_PARTKEY,
	PS_SUPPLYCOST,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS L,
                                    " + Constants.RightRelationNameTag + @" AS PS
                        WHERE
                        PS_SUPPKEY = L_SUPPKEY
	AND PS_PARTKEY = L_PARTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_QUANTITY", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_PARTKEY", FieldNames = new List<string>() { "L_PARTKEY" } },
                    })
                ,
                3,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectPS, "PS",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "PS_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "PS_SUPPKEY", FieldNames = new List<string>() { "PS_SUPPKEY"} },
                            new Index() {Name = "PS_PARTKEY", FieldNames = new List<string>() { "PS_PARTKEY" } },
                        })
                )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY,
	L_QUANTITY,
	PS_SUPPLYCOST,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS L,
                                    " + Constants.RightRelationNameTag + @" AS P
                        WHERE
                        P_PARTKEY = L_PARTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "L_QUANTITY", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY" } },
                    })
                ,
                2,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectP, "P",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "P_PARTKEY", FieldNames = new List<string>() { "P_PARTKEY"} },
                        })
                )));

            var join4 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_QUANTITY,
	PS_SUPPLYCOST,
	O_ORDERDATE,
	O_ORDERPRIORITY,
	S_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS L,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        O_ORDERKEY = L_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_QUANTITY", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                        new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                4,
                builder.CreateRelation(join3),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERDATE", Params = " DATE NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                        })
                )));

            var join5 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
    N_NAME AS NATION,
    EXTRACT(YEAR FROM O_ORDERDATE) AS O_YEAR,
    L_EXTENDEDPRICE * (1 - L_DISCOUNT) - PS_SUPPLYCOST * L_QUANTITY AS AMOUNT
FROM
                                    " + Constants.LeftRelationNameTag + @" AS L,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "NATION", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "O_YEAR", Params = " INT(4) NULL DEFAULT NULL"},
                        new Field() {Name = "AMOUNT", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                5,
                builder.CreateRelation(join4),
                builder.CreateRelation(selectN, "N",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                ),
                @"SELECT
    NATION,
    O_YEAR,
    AMOUNT
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	NATION,
	O_YEAR,
	SUM(AMOUNT) AS SUM_PROFIT
FROM
	" + Constants.RelationNameTag + @" AS PROFIT
GROUP BY
	NATION,
	O_YEAR
ORDER BY
	NATION,
	O_YEAR DESC;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "NATION", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "O_YEAR", Params = " INT(4) NULL DEFAULT NULL"},
                        new Field() {Name = "SUM_PROFIT", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	NATION,
	O_YEAR,
	SUM_PROFIT
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join5, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 10 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q10()
        {
            var builder = new QueryBuilder(10);

            #region SELECT

            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT,
	C_NATIONKEY
FROM
	CUSTOMER", 1));

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	O_CUSTKEY,
	O_ORDERKEY
FROM
	ORDERS
WHERE 
	O_ORDERDATE >= DATE '1994-04-01'
	AND O_ORDERDATE < DATE '1994-04-01' + INTERVAL '3' MONTH", 2));

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_ORDERKEY
FROM
	LINEITEM
WHERE
	L_RETURNFLAG = 'R'", 3));

            var selectN = builder.AddSelectQuery(builder.CreateSelectQuery(
                @"SELECT
	N_NAME,
	N_NATIONKEY
FROM
	NATION", 4));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT,
	C_NATIONKEY,
	O_ORDERKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS C,
                                    " + Constants.RightRelationNameTag + @" AS O
                        WHERE
                        C_CUSTKEY = O_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NAME", Params = " VARCHAR(25) NOT NULL"},
                        new Field() {Name = "C_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "C_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "C_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "C_COMMENT", Params = " VARCHAR(117) NOT NULL"},
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectC, "C",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "C_NAME", Params = " VARCHAR(25) NOT NULL"},
                            new Field() {Name = "C_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "C_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                            new Field() {Name = "C_PHONE", Params = " CHAR(15) NOT NULL"},
                            new Field() {Name = "C_COMMENT", Params = " VARCHAR(117) NOT NULL"},
                            new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                        })
                ),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	C_NATIONKEY
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J1,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        L_ORDERKEY = O_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NAME", Params = " VARCHAR(25) NOT NULL"},
                        new Field() {Name = "C_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "C_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "C_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "C_COMMENT", Params = " VARCHAR(117) NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "C_NATIONKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "C_NATIONKEY", FieldNames = new List<string>() { "C_NATIONKEY"} },
                    })
                ,
                2,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectL, "L", 
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                    })
                )));

            var join3 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N_NAME
FROM
                                    " + Constants.LeftRelationNameTag + @" AS J2,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        C_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NAME", Params = " VARCHAR(25) NOT NULL"},
                        new Field() {Name = "C_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "C_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "C_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "C_COMMENT", Params = " VARCHAR(117) NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                    }),
                3,
                builder.CreateRelation(join2),
                builder.CreateRelation(selectN, "N",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                ),
                @"SELECT
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT,
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	N_NAME
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	C_CUSTKEY,
	C_NAME,
	SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,
	C_ACCTBAL,
	N_NAME,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT
FROM
	" + Constants.RelationNameTag + @" AS PROFIT
GROUP BY
	C_CUSTKEY,
	C_NAME,
	C_ACCTBAL,
	C_PHONE,
	N_NAME,
	C_ADDRESS,
	C_COMMENT
ORDER BY
	REVENUE DESC;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "C_NAME", Params = " VARCHAR(25) NOT NULL"},
                        new Field() {Name = "REVENUE", Params = " DECIMAL(18,4) NULL DEFAULT NULL"},
                        new Field() {Name = "C_ACCTBAL", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "N_NAME", Params = " CHAR(25) NOT NULL"},
                        new Field() {Name = "C_ADDRESS", Params = " VARCHAR(40) NOT NULL"},
                        new Field() {Name = "C_PHONE", Params = " CHAR(15) NOT NULL"},
                        new Field() {Name = "C_COMMENT", Params = " VARCHAR(117) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	C_CUSTKEY,
	C_NAME,
	REVENUE,
	C_ACCTBAL,
	N_NAME,
	C_ADDRESS,
	C_PHONE,
	C_COMMENT
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join3, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 11 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q11()
        {
            var builder = new QueryBuilder(11);

            #region SELECT

            var selectPS = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	PS_PARTKEY,
	PS_SUPPKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
FROM
	PARTSUPP;", 1));
            var selectS = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	S_NATIONKEY,
	S_SUPPKEY
FROM
	SUPPLIER;", 2));
            var selectN = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	N_NATIONKEY
FROM
	NATION
WHERE
	N_NAME = 'ALGERIA';", 3));
            var selectPS2 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	PS_SUPPKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
FROM
	PARTSUPP;", 4));
            var selectS2 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	S_NATIONKEY,
	S_SUPPKEY
FROM
	SUPPLIER;", 5));

            var selectN2 = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	N_NATIONKEY
FROM
	NATION
WHERE
	N_NAME = 'ALGERIA';", 6));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	PS_PARTKEY,
	S_NATIONKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS PS,
                                    " + Constants.RightRelationNameTag + @" AS S
                        WHERE
                        PS_SUPPKEY = S_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectPS, "PS",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "PS_SUPPKEY", FieldNames = new List<string>() { "PS_SUPPKEY"} },
                        })
                ),
                builder.CreateRelation(selectS, "S",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                        })
                )));

            var join2 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	PS_PARTKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS J01,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "PS_PARTKEY", FieldNames = new List<string>() { "PS_PARTKEY"} },
                    })

                , 1,
                builder.CreateRelation(join1),
                builder.CreateRelation(selectN, "N",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                ),
                @"SELECT
	PS_PARTKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
FROM
    " + Constants.RelationNameTag + @""));

            var join11 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	S_NATIONKEY,
	PS_SUPPLYCOST,
	PS_AVAILQTY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS PS2,
                                    " + Constants.RightRelationNameTag + @" AS S2
                        WHERE
                        PS_SUPPKEY = S_SUPPKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "S_NATIONKEY", FieldNames = new List<string>() { "S_NATIONKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectPS2, "PS2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "PS_SUPPKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "PS_SUPPKEY", FieldNames = new List<string>() { "PS_SUPPKEY"} },
                        })
                ),
                builder.CreateRelation(selectS2, "S2", builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "S_NATIONKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "S_SUPPKEY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index(true) {Name = "S_SUPPKEY", FieldNames = new List<string>() { "S_SUPPKEY"} },
                    })
                )));

            var join12 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	PS_SUPPLYCOST,
	PS_AVAILQTY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS J11,
                                    " + Constants.RightRelationNameTag + @" AS N
                        WHERE
                        S_NATIONKEY = N_NATIONKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PS_SUPPLYCOST", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "PS_AVAILQTY", Params = " INT NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                builder.CreateRelation(join11),
                builder.CreateRelation(selectN2, "N2",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "N_NATIONKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "N_NATIONKEY", FieldNames = new List<string>() { "N_NATIONKEY"} },
                        })
                ),
                @"SELECT
	PS_SUPPLYCOST,
	PS_AVAILQTY
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	PS_PARTKEY,
	SUM(PS_SUPPLYCOST * PS_AVAILQTY) AS VALUE
FROM
	" + Constants.RelationNameTag + @"
GROUP BY
	PS_PARTKEY HAVING
		SUM(PS_SUPPLYCOST * PS_AVAILQTY) > (
			SELECT
				SUM(PS_SUPPLYCOST * PS_AVAILQTY) * 0.0001000000
			FROM
				" + Constants.RelationNameTag + @"
		)
ORDER BY
	VALUE DESC;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PS_PARTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "VALUE", Params = " DECIMAL(18,2) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	PS_PARTKEY,
	VALUE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join2, QueryRelationStatus.Wait), builder.CreateRelation(join12, QueryRelationStatus.Wait)));

            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 12 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q12()
        {
            var builder = new QueryBuilder(12);

            #region SELECT

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	O_ORDERKEY,
	O_ORDERPRIORITY
FROM
	ORDERS;", 1));
            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	L_ORDERKEY,
	L_SHIPMODE
FROM
	LINEITEM
WHERE
	L_SHIPMODE IN ('AIR', 'SHIP')
	AND L_COMMITDATE < L_RECEIPTDATE
	AND L_SHIPDATE < L_COMMITDATE
	AND L_RECEIPTDATE >= DATE '1994-01-01'
	AND L_RECEIPTDATE < DATE '1994-01-01' + INTERVAL '1' YEAR;", 2));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	L_SHIPMODE, 
	O_ORDERPRIORITY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS O,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
                        O_ORDERKEY = L_ORDERKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPMODE", Params = " CHAR(10) NOT NULL"},
                        new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERPRIORITY", Params = " CHAR(15) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                        })
                ),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_ORDERKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "L_SHIPMODE", Params = " CHAR(10) NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_ORDERKEY", FieldNames = new List<string>() { "L_ORDERKEY"} },
                        })
                ),
                @"SELECT
	L_SHIPMODE, 
	O_ORDERPRIORITY
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	L_SHIPMODE,
	SUM(CASE
		WHEN O_ORDERPRIORITY = '1-URGENT'
			OR O_ORDERPRIORITY = '2-HIGH'
			THEN 1
		ELSE 0
	END) AS HIGH_LINE_COUNT,
	SUM(CASE
		WHEN O_ORDERPRIORITY <> '1-URGENT'
			AND O_ORDERPRIORITY <> '2-HIGH'
			THEN 1
		ELSE 0
	END) AS LOW_LINE_COUNT
FROM
	" + Constants.RelationNameTag + @"
GROUP BY
	L_SHIPMODE
ORDER BY
	L_SHIPMODE;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "L_SHIPMODE", Params = " CHAR(10) NOT NULL"},
                        new Field() {Name = "HIGH_LINE_COUNT", Params = " DECIMAL(18,0) NULL DEFAULT NULL"},
                        new Field() {Name = "LOW_LINE_COUNT", Params = " DECIMAL(18,0) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	L_SHIPMODE,
	HIGH_LINE_COUNT,
	LOW_LINE_COUNT
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join1, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }
        
        /// <summary>
        ///     Запрос № 13 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q13()
        {
            var builder = new QueryBuilder(13);

            #region SELECT

            var selectO = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	O_CUSTKEY,
	O_ORDERKEY
FROM
	ORDERS
WHERE
	O_COMMENT NOT LIKE '%SPECIAL%REQUESTS%';", 1));
            var selectC = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	C_CUSTKEY
FROM
	CUSTOMER;", 2));


            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	C_CUSTKEY, 
	O_ORDERKEY
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS C LEFT OUTER JOIN
                                    " + Constants.RightRelationNameTag + @" AS O
                        ON
                        C_CUSTKEY = O_CUSTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        new Field() {Name = "O_ORDERKEY", Params = " INT NULL"},
                    },
                    new List<Index>()
                    {
                        new Index() {Name = "O_ORDERKEY", FieldNames = new List<string>() { "O_ORDERKEY"} },
                    })
                ,
                1,
                builder.CreateRelation(selectC, "C",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "C_CUSTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "C_CUSTKEY", FieldNames = new List<string>() { "C_CUSTKEY"} },
                        })
                ),
                builder.CreateRelation(selectO, "O",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "O_CUSTKEY", Params = " INT NOT NULL"},
                            new Field() {Name = "O_ORDERKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "O_CUSTKEY", FieldNames = new List<string>() { "O_CUSTKEY"} },
                        })
                ),
                @"SELECT
	C_CUSTKEY, 
	O_ORDERKEY
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	C_COUNT,
	COUNT(*) AS CUSTDIST
FROM
	(SELECT
	C_CUSTKEY,
	COUNT(O_ORDERKEY) AS C_COUNT
FROM
	" + Constants.RelationNameTag + @"
GROUP BY
	C_CUSTKEY) AS SORT
GROUP BY
	C_COUNT
ORDER BY
	CUSTDIST DESC,
	C_COUNT DESC;",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "C_COUNT", Params = " BIGINT NOT NULL DEFAULT '0'"},
                        new Field() {Name = "CUSTDIST", Params = " BIGINT NOT NULL DEFAULT '0'"},
                    },
                    new List<Index>()
                    {
                    })
                ,
                1,
                @"SELECT
	C_COUNT,
	CUSTDIST
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join1, QueryRelationStatus.Wait)));

            #endregion
            
            return builder.GetQuery();
        }

        /// <summary>
        ///     Запрос № 14 из теста TPC-H
        /// </summary>
        /// <returns>новый запрос на выполнение</returns>
        protected override Query Q14()
        {
            var builder = new QueryBuilder(14);

            #region SELECT

            var selectL = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	L_EXTENDEDPRICE,
	L_DISCOUNT,
	L_PARTKEY
FROM
	LINEITEM
WHERE
	L_SHIPDATE >= DATE '1995-01-01'
	AND L_SHIPDATE < DATE '1995-01-01' + INTERVAL '1' MONTH;", 1));
            var selectP = builder.AddSelectQuery(builder.CreateSelectQuery(@"SELECT
	P_TYPE,
	P_PARTKEY
FROM
	PART;", 2));

            #endregion

            #region JOIN

            var join1 = builder.AddJoinQuery(builder.CreateJoinQuery(@"SELECT
	P_TYPE,
	L_EXTENDEDPRICE,
	L_DISCOUNT
                                FROM
                                    " + Constants.LeftRelationNameTag + @" AS P,
                                    " + Constants.RightRelationNameTag + @" AS L
                        WHERE
	L_PARTKEY = P_PARTKEY",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "P_TYPE", Params = " VARCHAR(25) NOT NULL"},
                        new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                        new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                    },
                    new List<Index>()
                    {
                    })
            , 1,
                builder.CreateRelation(selectP, "P",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "P_TYPE", Params = " VARCHAR(25) NOT NULL"},
                            new Field() {Name = "P_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index(true) {Name = "P_PARTKEY", FieldNames = new List<string>() { "P_PARTKEY"} },
                        })
                ),
                builder.CreateRelation(selectL, "L",
                    builder.CreateRelationSchema(
                        new List<Field>()
                        {
                            new Field() {Name = "L_EXTENDEDPRICE", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_DISCOUNT", Params = " DECIMAL(15,2) NOT NULL"},
                            new Field() {Name = "L_PARTKEY", Params = " INT NOT NULL"},
                        },
                        new List<Index>()
                        {
                            new Index() {Name = "L_PARTKEY", FieldNames = new List<string>() { "L_PARTKEY"} },
                        })
                ),
                @"SELECT
	P_TYPE,
	L_EXTENDEDPRICE,
	L_DISCOUNT
FROM
    " + Constants.RelationNameTag + @""));

            #endregion

            #region SORT

            builder.SetSortQuery(builder.CreateSortQuery(@"SELECT
	100.00 * SUM(CASE
		WHEN P_TYPE LIKE 'PROMO%'
			THEN L_EXTENDEDPRICE * (1 - L_DISCOUNT)
		ELSE 0
	END) / SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS PROMO_REVENUE
FROM
	" + Constants.RelationNameTag + @"",
                builder.CreateRelationSchema(
                    new List<Field>()
                    {
                        new Field() {Name = "PROMO_REVENUE", Params = " DECIMAL(18,10) NULL DEFAULT NULL"},
                    },
                    new List<Index>()
                    {
                    })
                ,
    1,
                @"SELECT
	PROMO_REVENUE
FROM
	" + Constants.RelationNameTag + @";",
                builder.CreateRelation(join1, QueryRelationStatus.Wait)));

            #endregion

            return builder.GetQuery();
        }
        #endregion
    }
}