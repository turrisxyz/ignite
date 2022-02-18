/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.apache.ignite.internal.processors.query.calcite.integration;

import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Function;
import java.util.function.Predicate;
import java.util.function.Supplier;
import org.apache.calcite.util.ImmutableBitSet;
import org.apache.ignite.internal.processors.query.calcite.QueryChecker;
import org.apache.ignite.internal.processors.query.calcite.exec.ExecutionContext;
import org.apache.ignite.internal.processors.query.calcite.metadata.ColocationGroup;
import org.apache.ignite.internal.processors.query.calcite.schema.IgniteTable;
import org.jetbrains.annotations.Nullable;
import org.junit.Test;

/**
 * Index scan test.
 */
public class IndexScanlIntegrationTest extends AbstractBasicIntegrationTest {
    /** {@inheritDoc} */
    @Override protected int nodeCount() {
        return 1;
    }

    /** */
    @Test
    public void testNullsInSearchRow() {
        executeSql("CREATE TABLE t(i1 INTEGER, i2 INTEGER) WITH TEMPLATE=REPLICATED ");
        executeSql("INSERT INTO t VALUES (0, null), (1, null), (2, 2), (3, null), (4, null)");
        executeSql("CREATE INDEX t_idx ON t(i1)");

        IgniteTable tbl = (IgniteTable)queryProcessor(grid(0)).schemaHolder().schema("PUBLIC").getTable("T");

        AtomicInteger filteredRows = new AtomicInteger();
        tbl.addIndex(new DelegatingIgniteIndex(tbl.getIndex("T_IDX")) {
            @Override public <Row> Iterable<Row> scan(
                ExecutionContext<Row> execCtx,
                ColocationGroup grp,
                Predicate<Row> filters,
                Supplier<Row> lowerIdxConditions,
                Supplier<Row> upperIdxConditions,
                Function<Row, Row> rowTransformer,
                @Nullable ImmutableBitSet requiredColumns
            ) {
                Predicate<Row> filter = row -> {
                    filteredRows.incrementAndGet();

                    return true;
                };

                filters = filter.and(filters);

                return delegate.scan(execCtx, grp, filters, lowerIdxConditions, upperIdxConditions, rowTransformer,
                    requiredColumns);
            }
        });

        String sql = "SELECT /*+ DISABLE_RULE('NestedLoopJoinConverter', 'MergeJoinConverter') */ t1.i1, t2.i1 " +
            "FROM t t1 " +
            "LEFT JOIN t t2 ON t1.i2 = t2.i1";

        assertQuery(sql)
            .matches(QueryChecker.containsSubPlan("IgniteCorrelatedNestedLoopJoin"))
            .matches(QueryChecker.containsIndexScan("PUBLIC", "T", "T_IDX"))
            .returns(0, null)
            .returns(1, null)
            .returns(2, 2)
            .returns(3, null)
            .returns(4, null)
            .check();

        // There shouldn't be full index scan in case of null values in search row, only one value must be found by
        // range scan and passed to predicate.
        assertEquals(1, filteredRows.get());
    }
}
